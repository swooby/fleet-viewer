using UnityEngine;
using System.Collections.Generic;

// TODO:(pv) Mark as static so that update/etc isn't called on us...
public class ModelFactory
{
    private ModelFactory()
    {
    }

    private static Dictionary<string, GameObject> models = new Dictionary<string, GameObject>();

    public static GameObject Get(string modelPath)
    {
        GameObject model;

        if (!models.TryGetValue(modelPath, out model))
        {
            //Debug.LogError("ModelFactory.Get: Non-Cached Load: CTMReader.Read(...)");
            model = CTMReader.Read(modelPath);

            models[modelPath] = model;
        }
        else
        {
            //Debug.LogError("ModelFactory.Get: Cached Load: Object.Instantiate(model)");
			model = Object.Instantiate(model);

			model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			model.transform.localScale = Vector3.one;
		}

        return model;
    }

    public static void Release(GameObject model)
    {
        Object.Destroy(model);
    }
}
