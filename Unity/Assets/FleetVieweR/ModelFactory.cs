using UnityEngine;
using System.Collections.Generic;

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
        models.TryGetValue(modelPath, out cachedModel);
        return cachedModel;
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

                            LoadModelCallback tempCallback;
                            lock (modelPathCallbacks)
                            {
								int thisModelPathCallbacksCount = thisModelPathCallbacks.Count;
								Debug.Log("ModelFactory.LoadModelAsync: thisModelPathCallbacks.Count:" + thisModelPathCallbacksCount);
								while (thisModelPathCallbacksCount > 0)
                                {
                                    model = OnModelLoaded(modelPath, model);

                                    tempCallback = thisModelPathCallbacks[0];
                                    tempCallback(model);
                                    thisModelPathCallbacks.RemoveAt(0);

									thisModelPathCallbacksCount = thisModelPathCallbacks.Count;
									Debug.Log("ModelFactory.LoadModelAsync: thisModelPathCallbacks.Count:" + thisModelPathCallbacksCount);
								}
                            }
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
