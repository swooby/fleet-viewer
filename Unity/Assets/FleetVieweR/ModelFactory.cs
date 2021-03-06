using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FleetVieweR
{
    /// <summary>
    /// ModelFactory loads 
    /// 
    /// TODO:(pv) Create LODGroup and load for each "fvLOD#" file that exists
    /// </summary>
    public class ModelFactory
    {
        private static readonly string TAG = Utils.TAG<ModelFactory>();

        public const bool VERBOSE_LOG = false;

        private ModelFactory()
        {
        }

        private static Dictionary<string, GameObject> models = new Dictionary<string, GameObject>();

        private static GameObject GetCachedModel(string modelPath)
        {
            GameObject cachedModel;
            return models.TryGetValue(modelPath, out cachedModel) ? cachedModel : null;
        }

        private static List<LoadModelCallback> GetModelPathCallbacks(string modelPath)
        {
            List<LoadModelCallback> thisModelPathCallbacks;
            if (!modelPathCallbacks.TryGetValue(modelPath, out thisModelPathCallbacks))
            {
                thisModelPathCallbacks = new List<LoadModelCallback>();
                modelPathCallbacks[modelPath] = thisModelPathCallbacks;
            }
            return thisModelPathCallbacks;
        }

        public delegate void LoadModelCallback(GameObject model);

        private static Dictionary<string, List<LoadModelCallback>> modelPathCallbacks = new Dictionary<string, List<LoadModelCallback>>();

        public static void LoadModelAsync(string modelPath, LoadModelCallback callback)
        {
            if (VERBOSE_LOG)
            {
                Debug.Log(TAG + " +ModelFactory.LoadModelAsync(modelPath:" + Utils.Quote(modelPath) +
                           ", callback:" + callback + ")");
            }

            GameObject cachedModel = GetCachedModel(modelPath);
            if (cachedModel == null)
            {
                lock (modelPathCallbacks)
                {
                    //
                    // Double-check now that we got the lock
                    //
                    cachedModel = GetCachedModel(modelPath);
                    if (cachedModel == null)
                    {
                        List<LoadModelCallback> thisModelPathCallbacks = GetModelPathCallbacks(modelPath);

                        thisModelPathCallbacks.Add(callback);

                        if (thisModelPathCallbacks.Count == 1)
                        {
                            if (VERBOSE_LOG)
                            {
                                Debug.LogError(TAG + " ModelFactory.LoadModelAsync: Non-Cached Load: CTMReader.LoadAsync(...)");
                            }
                            InternalLoadModelAsync(modelPath, (model) =>
                            {
                                if (VERBOSE_LOG)
                                {
                                    Debug.LogError(TAG + " ModelFactory.LoadModelAsync: Non-Cached Load completed; model:" + model);
                                }
                                if (model == null)
                                {
                                    return;
                                }

                                List<GameObject> lodModels = new List<GameObject>();
                                lodModels.Add(model);

                                LoadModelLodAsync(lodModels, modelPath, 1, () =>
                                {
                                    GameObject lodRoot = CreateModelLods(lodModels);

                                    LoadModelCallback tempCallback;
                                    lock (modelPathCallbacks)
                                    {
                                        while (thisModelPathCallbacks.Count > 0)
                                        {
                                            OnModelLoaded(modelPath, lodRoot);

                                            tempCallback = thisModelPathCallbacks[0];
                                            tempCallback(lodRoot);
                                            thisModelPathCallbacks.RemoveAt(0);
                                        }
                                    }
                                });
                            });
                        }
                        else
                        {
                            if (VERBOSE_LOG)
                            {
                                Debug.LogError(TAG + " ModelFactory.LoadModelAsync: Non-Cached Load already in progress");
                            }
                        }

                        return;
                    }
                }
            }

            if (VERBOSE_LOG)
            {
                Debug.LogError(TAG + " ModelFactory.LoadModelAsync: Cached Load");
            }

            GameObject loadedModel = OnModelLoaded(modelPath, cachedModel);

            callback(loadedModel);

            if (VERBOSE_LOG)
            {
                Debug.Log(TAG + " -ModelFactory.LoadModelAsync(modelPath:" + Utils.Quote(modelPath) +
                          ", callback:" + callback + ")");
            }
        }

        private static void InternalLoadModelAsync(string modelPath, CTMReader.LoadCallback callback)
        {
            ConfigInfo.EnsureFileCached(modelPath, (cachedPath) =>
            {
                if (String.IsNullOrEmpty(cachedPath))
                {
                    if (VERBOSE_LOG)
                    {
                        Debug.Log(TAG + " EnsureFileCached: " + Utils.Quote(modelPath) + " not cached; trying local resource");
                    }
                    CTMReader.LoadResourceAsync(modelPath, callback);
                }
                else
                {
                    if (VERBOSE_LOG)
                    {
                        Debug.Log(TAG + " EnsureFileCached: " + Utils.Quote(modelPath) + " cached; loading cached file");
                    }
                    CTMReader.LoadFileAsync(cachedPath, callback);
                }
            });
        }

        //
        // LOD ideas:
        //  https://www.youtube.com/watch?v=IzlU_xvTK3Y
        //  https://www.youtube.com/watch?v=T2QstH7GQxU
        //

        private static void LoadModelLodAsync(List<GameObject> lodModels, string modelPath, int lod, Action callback)
        {
            string directory = Path.GetDirectoryName(modelPath);
            string filename = Path.GetFileNameWithoutExtension(modelPath);
            string ext = Path.GetExtension(modelPath);

            if (!filename.EndsWith("fv_LOD0"))
            {
                callback();
                return;
            }

            // TODO:(pv) Load LOD names from touched single index file to potentially void hitting Firebase to check each LOD file

            filename = filename.Substring(0, filename.Length - 1) + lod;

            string resourcePath = directory + "/" + filename + ext;
            if (VERBOSE_LOG)
            {
                Debug.Log("ModelFactory.LoadModelLodAsync: resourcePath:" + Utils.Quote(resourcePath));
            }

            InternalLoadModelAsync(resourcePath, (model) =>
            {
                if (VERBOSE_LOG)
                {
                    Debug.LogError(TAG + " ModelFactory.LoadModelLodAsync: CTMReader.LoadAsync completed; model:" + model);
                }
                if (model == null)
                {
                    callback();
                }
                else
                {
                    lodModels.Add(model);

                    LoadModelLodAsync(lodModels, modelPath, lod + 1, callback);
                }
            });
        }

        private static GameObject CreateModelLods(List<GameObject> lodModels)
        {
            GameObject lodRoot = new GameObject();
            LODGroup lodGroup = lodRoot.AddComponent<LODGroup>();

            int lodCount = lodModels.Count;

            LOD[] lods = new LOD[lodCount];
            //Debug.LogError(TAG + " ModelFactory.AddLod: lods:" + lods);

            int i = 0;
            float screenRelativeTransitionHeight = 1.0f;
            foreach (GameObject lodModel in lodModels)
            {
                lodModel.name = "_LOD" + i;
                lodModel.transform.parent = lodRoot.transform;
                Renderer[] renderers = lodModel.GetComponentsInChildren<Renderer>(true);
                if (i == lodCount - 1)
                {
                    screenRelativeTransitionHeight = 0.0f;
                }
                else
                {
                    screenRelativeTransitionHeight *= 0.5f;
                }
                lods[i] = new LOD(screenRelativeTransitionHeight, renderers);

                i++;
            }

            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();

            return lodRoot;
        }

        private static GameObject OnModelLoaded(string modelPath, GameObject model)
        {
            if (VERBOSE_LOG)
            {
                Debug.Log(TAG + " +ModelFactory.OnModelLoaded(modelPath:" + Utils.Quote(modelPath) +
                          ", model:" + model + ")");
            }

            GameObject cachedModel = GetCachedModel(modelPath);
            if (cachedModel == null)
            {
                models[modelPath] = model;
            }
            else
            {
                model = UnityEngine.Object.Instantiate(cachedModel);

                model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                model.transform.localScale = Vector3.one;
            }

            if (VERBOSE_LOG)
            {
                Debug.Log(TAG + " -ModelFactory.OnModelLoaded(modelPath:" + Utils.Quote(modelPath) +
                          ", model:" + model + ")");
            }

            return model;
        }
    }
}