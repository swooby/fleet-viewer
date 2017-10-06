using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// ModelFactory loads 
/// 
/// TODO:(pv) Create LODGroup and load for each "fvLOD#" file that exists
/// </summary>
public class ModelFactory
{
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
        Debug.Log("+ModelFactory.LoadModelAsync(modelPath:" + Utils.Quote(modelPath) +
                   ", callback:" + callback + ")");

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
                            Debug.LogError("ModelFactory.LoadModelAsync: Non-Cached Load: CTMReader.LoadAsync(...)");
                        }
                        CTMReader.LoadAsync(modelPath, (model) =>
                        {
                            if (VERBOSE_LOG)
                            {
                                Debug.LogError("ModelFactory.LoadModelAsync: CTMReader.LoadAsync completed");
                            }
                            if (model == null)
                            {
                                return;
                            }

                            GameObject lodRoot = new GameObject();

                            AddLod(lodRoot, model);

                            //LoadModelLodAsync(lodRoot, modelPath, 1, () =>
                            //{
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
                            //});
                        });
                    }
                    else
                    {
                        if (VERBOSE_LOG)
                        {
                            Debug.LogError("ModelFactory.LoadModelAsync: CTMReader.LoadAsync already in progress");
                        }
                    }

                    return;
                }
            }
        }

        if (VERBOSE_LOG)
        {
            Debug.LogError("ModelFactory.LoadModelAsync: Cached Load");
        }

        GameObject loadedModel = OnModelLoaded(modelPath, cachedModel);

        callback(loadedModel);

        Debug.Log("-ModelFactory.LoadModelAsync(modelPath:" + Utils.Quote(modelPath) +
                  ", callback:" + callback + ")");
    }

    private static void LoadModelLodAsync(GameObject lodRoot, string modelPath, int lod, Action callback)
    {
        string filename = Path.GetFileNameWithoutExtension(modelPath);
        string ext = Path.GetExtension(modelPath);

        string resourcePath = filename + "-fvLOD" + lod + ext;
        Debug.Log("ModelFactory.LoadModelLodAsync: resourcePath:" + Utils.Quote(resourcePath));

        CTMReader.LoadAsync(resourcePath, (model) =>
        {
            if (VERBOSE_LOG)
            {
                Debug.LogError("ModelFactory.LoadModelLodAsync: CTMReader.LoadAsync completed");
            }
            if (model == null)
            {
                callback();
            }
            else
            {
                AddLod(lodRoot, model);

                LoadModelLodAsync(lodRoot, modelPath, lod + 1, callback);
            }
        });
    }

    private static void AddLod(GameObject lodRoot, GameObject lodModel)
    {
        LODGroup lodGroup = lodRoot.GetComponent<LODGroup>();
        if (lodGroup == null)
        {
            lodGroup = lodRoot.AddComponent<LODGroup>();
            lodGroup.SetLODs(null);
        }

        List<LOD> lods = new List<LOD>(lodGroup.GetLODs());

        lodModel.name = "LOD " + lods.Count;
        lodModel.transform.parent = lodRoot.transform;
        Renderer[] renderers = new Renderer[1];
        renderers[0] = lodModel.GetComponent<Renderer>();
        LOD lod = new LOD(0.0f, renderers);

        lods.Add(lod);

        int i = 0;
        List<LOD>.Enumerator enumerator = lods.GetEnumerator();
        while (enumerator.MoveNext())
        {
            lod = enumerator.Current;
            lod.screenRelativeTransitionHeight = 1.0f / (i + 1);
            i++;
        }

        lodGroup.SetLODs(lods.ToArray());
        lodGroup.RecalculateBounds();
    }

    private static GameObject OnModelLoaded(string modelPath, GameObject model)
    {
        Debug.Log("+ModelFactory.OnModelLoaded(modelPath:" + Utils.Quote(modelPath) +
                  ", model:" + model + ")");

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

        Debug.Log("-ModelFactory.OnModelLoaded(modelPath:" + Utils.Quote(modelPath) +
                  ", model:" + model + ")");

        return model;
    }
}
