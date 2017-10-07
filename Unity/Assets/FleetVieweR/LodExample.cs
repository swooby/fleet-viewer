using UnityEngine;
using System.Collections;

public class LodExample : MonoBehaviour
{
    public LODGroup group;

	void Start()
	{
		group = gameObject.AddComponent<LODGroup>();

		LOD[] lods = new LOD[4];

        if (false)
        {
			//
			// From https://docs.unity3d.com/ScriptReference/LODGroup.SetLODs.html
            //  Doesn't work as expected.
            //  LOD0 isn't correct.
			//
			for (int i = 0, lodCount = lods.Length; i < lodCount; i++)
			{
				PrimitiveType primType = PrimitiveType.Cube;
				switch (i)
				{
					case 1:
						primType = PrimitiveType.Capsule;
						break;
					case 2:
						primType = PrimitiveType.Sphere;
						break;
					case 3:
						primType = PrimitiveType.Cylinder;
						break;
				}
				GameObject go = GameObject.CreatePrimitive(primType);
				go.transform.parent = gameObject.transform;
				Renderer[] renderers = new Renderer[1];
				renderers[0] = go.GetComponent<Renderer>();
				lods[i] = new LOD(1.0F / (i + 1), renderers);
			}
        }
        else
        {
            float screenRelativeTransitionHeight = 1.0f;
            for (int i = 0, lodCount = lods.Length; i < lodCount; i++)
            {
                PrimitiveType primType = PrimitiveType.Capsule;
                switch (i)
                {
                    case 1:
                        primType = PrimitiveType.Cylinder;
                        break;
                    case 2:
                        primType = PrimitiveType.Sphere;
                        break;
                    case 3:
                        primType = PrimitiveType.Cube;
                        break;
                }
                GameObject go = GameObject.CreatePrimitive(primType);
                go.transform.parent = gameObject.transform;
                Renderer[] renderers = new Renderer[1];
                renderers[0] = go.GetComponent<Renderer>();
                if (i == lodCount - 1)
                {
                    screenRelativeTransitionHeight = 0.0f;
                }
                else
                {
                    screenRelativeTransitionHeight *= 0.5f;
                }
                Debug.LogWarning("LOD" + i + ": screenRelativeTransitionHeight:" + screenRelativeTransitionHeight);
                lods[i] = new LOD(screenRelativeTransitionHeight, renderers);
            }
        }
		group.SetLODs(lods);
		group.RecalculateBounds();
    }

    void OnGUI()
	{
		if (GUILayout.Button("Enable / Disable"))
			group.enabled = !group.enabled;

		if (GUILayout.Button("Default"))
			group.ForceLOD(-1);

		if (GUILayout.Button("Force 0"))
			group.ForceLOD(0);

		if (GUILayout.Button("Force 1"))
			group.ForceLOD(1);

		if (GUILayout.Button("Force 2"))
			group.ForceLOD(2);

		if (GUILayout.Button("Force 3"))
			group.ForceLOD(3);

		if (GUILayout.Button("Force 4"))
			group.ForceLOD(4);

		if (GUILayout.Button("Force 5"))
			group.ForceLOD(5);

		if (GUILayout.Button("Force 6"))
			group.ForceLOD(6);
	}
}
