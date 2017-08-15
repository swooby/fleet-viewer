using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;
using OpenCTM;

public class FleetSceneManager : MonoBehaviour
{
    #region DATA

    [Serializable]
    private class AppSettings
    {
        public string SystemName;
        public List<ModelSettings> ModelSettings = new List<ModelSettings>();

    }

	[Serializable]
	private class Vector3Serializable
    {
		public float x;
		public float y;
		public float z;
	}

	[Serializable]
	private class QuaternionSerializable
	{
		public float x;
		public float y;
		public float z;
		public float w;
	}

	[Serializable]
    private class ModelSettings
    {
        public string Key;
        public Vector3Serializable Position;
        public QuaternionSerializable Rotation;
    }

    #endregion DATA

    private string AppSettingsPath;

    private AppSettings Settings;

    public SortedDictionary<string, ModelInfo> ModelInfos { get; private set; }

    void Start()
    {
		Input.backButtonLeavesApp = true;

        /*
		AppSettingsPath = Application.persistentDataPath + "/settings.fvr";
        Debug.Log("Start: AppSettingsPath == " + Utils.Quote(AppSettingsPath));

        if (false)
        {
            File.Delete(AppSettingsPath);
        }

        LoadAppSettings(true);

        LoadSystem(Settings.SystemName);

        foreach(ModelSettings modelSettings in Settings.ModelSettings)
        {
            LoadModel(modelSettings);
		}
		*/
    }

	void Update()
	{
		// Exit when (X) is tapped.
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	private void LoadAppSettings(bool handleException)
    {
        Debug.Log("LoadAppSettings(handleException:" + handleException + ")");
		try
        {
            using (FileStream file = File.Open(AppSettingsPath, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                Settings = (AppSettings)bf.Deserialize(file);
            }
        }
        catch(Exception e)
        {
            Debug.LogWarning("LoadAppSettings: " + e);

            if (handleException)
            {
                Settings = new AppSettings();
                Settings.SystemName = "Star Citizen";

                ModelSettings modelSettings = new ModelSettings();
                modelSettings.Key = "nox";
                Settings.ModelSettings.Add(modelSettings);

                try
                {
                    SaveAppSettings();
					LoadAppSettings(false);
				}
                catch
                {
                    throw;
                }
            }
		}
    }

    private void SaveAppSettings()
    {
		Debug.Log("SaveAppSettings()");
        try
        {
            using (FileStream file = File.Create(AppSettingsPath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, Settings);
            }
        }
        catch(Exception e)
        {
			Debug.LogWarning("SaveAppSettings: " + e);
		}
	}

    private void LoadSystem(string systemName)
    {
        Debug.Log("LoadSystem(systemName:" + Utils.Quote(systemName) + ")");

        SystemInfo system;
        SortedDictionary<string, SystemInfo> systems = new SortedDictionary<string, SystemInfo>(StringComparer.OrdinalIgnoreCase);
        CSVReader.Read<SystemInfo>("Systems", (dictionary) =>
        {
            system = new SystemInfo(dictionary);
            systems[system.Name] = system;
            return null;
        });

        systems.TryGetValue(systemName, out system);
        if (system == null)
        {
            Debug.LogError("LoadSystem: Failed to load systemName == " + Utils.Quote(systemName));
        }

        string configPath = system.ConfigPath;

        LoadModelInfos(configPath);
    }

    public void LoadModelInfos(string configPath)
    {
		Debug.Log("LoadModelInfos(configPath:" + Utils.Quote(configPath) + ")");

		ModelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);
        CSVReader.Read<ModelInfo>(configPath, (dictionary) =>
        {
            ModelInfo modelInfo = new ModelInfo(dictionary);
            ModelInfos[modelInfo.Name] = modelInfo;
            return null;
        });
        Debug.Log("LoadModelInfos: ModelInfos.Count == " + ModelInfos.Count);

        /*
        GameObject modelInfosDropdown = GameObject.Find("/Player/Main Camera/Overlay Canvas/ModelInfosDropdown");
        if (modelInfosDropdown == null)
        {
            Debug.LogWarning("LoadModelInfos: modelInfosDropdown == null; ignoring");
            return;
        }

        GvrDropdown dropdown = modelInfosDropdown.GetComponent<GvrDropdown>();
        if (dropdown == null)
        {
            Debug.LogWarning("LoadModelInfos: dropdown == null; ignoring");
            return;
        }

        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        foreach (ModelInfo modelInfo in ModelInfos.Values)
        {
            if (modelInfo == null || modelInfo.ModelPathRemote == null)
            {
                continue;
            }

            Dropdown.OptionData optionData = new Dropdown.OptionData()
            {
                text = modelInfo.Name
            };
            optionDatas.Add(optionData);
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(optionDatas);
        dropdown.value = 0;

        Debug.Log("LoadModelInfos: dropdown.options.Count == " + dropdown.options.Count);
        */
    }

    private GameObject LoadModel(ModelSettings modelSettings)
    {
		string modelKey = modelSettings.Key;

        GameObject model = LoadModel(modelKey);

        /*
		// TODO:(pv) Auto-arrange/position according to scale and previously loaded models...

		Vector3 modelPosition = modelSettings.Position;
        if (modelPosition != null)
        {
            model.transform.position = modelPosition;
        }

        Quaternion modelRotation = modelSettings.Rotation;
        if (modelRotation != null)
        {
            model.transform.rotation = modelRotation;
        }

		modelSettings.Position = modelPosition;
		modelSettings.Rotation = modelRotation;
        */

        return model;
	}

	private GameObject LoadModel(string modelKey)
	{
		ModelInfo modelInfo;
		if (!ModelInfos.TryGetValue(modelKey, out modelInfo) || modelInfo == null)
		{
            Debug.LogError("LoadScalePositionModel: Failed to load modelKey == " + Utils.Quote(modelKey));
			return null;
		}

		float modelLengthMeters = modelInfo.LengthMeters;
		Debug.Log("LoadScalePositionModel: modelLengthMeters == " + modelLengthMeters);
		string modelPathLocal = modelInfo.ModelPathLocal;
		Debug.Log("LoadScalePositionModel: modelPathLocal == " + modelPathLocal);
		Vector3 modelRotation = modelInfo.ModelRotation;
		Debug.Log("LoadScalePositionModel: modelRotation == " + modelRotation);

        GameObject go = LoadCTM(modelPathLocal);

		Transform goTransform = go.transform;

		Bounds goBounds = CalculateBounds(go);
		//Debug.LogError("LoadScalePositionModel: BEFORE goBounds == " + goBounds);
		float scale = modelLengthMeters / (goBounds.extents.z * 2);
		//Debug.LogError("LoadScalePositionModel: scale == " + scale);
		goTransform.localScale = new Vector3(scale, scale, scale);
		goBounds = CalculateBounds(go);
		//Debug.LogError("LoadScalePositionModel: AFTER goBounds == " + goBounds);

		//float goLengthMeters = goBounds.extents.z * 2;
		//Debug.LogError("LoadScalePositionModel: goLengthMeters == " + goLengthMeters);

		goTransform.Rotate(modelRotation);

		return go;
	}

	private class MeshInfo
	{
		public List<Vector3> vertices = new List<Vector3>();
		public List<Vector3> normals = new List<Vector3>();
		public List<int> triangles = new List<int>();
		public List<Vector2> uv = new List<Vector2>();
	}

	// TODO:(pv) Make this [or upstream caller] an async task so that we don't block...
	//  https://www.google.com/search?q=unity+async+load
	public GameObject LoadCTM(string resourcePath)
	{
		Debug.Log("CTMReader.Read(resourcePath:" + Utils.Quote(resourcePath) + ")");
		Debug.LogWarning("CTMReader.Read: MAX_TRIANGLES_PER_MESH == " + CTMReader.MAX_TRIANGLES_PER_MESH);

		GameObject root = new GameObject();

		TextAsset textAsset = Resources.Load(resourcePath) as TextAsset;
		Stream memoryStream = new MemoryStream(textAsset.bytes);

		//Debug.Log("CTMReader.Read: new CtmFileReader(memoryStream)");
		CtmFileReader reader = new CtmFileReader(memoryStream);
		//Debug.Log("CTMReader.Read: reader.decode()");
		OpenCTM.Mesh ctmMesh = reader.decode();
		//Debug.Log("CTMReader.Read: ctmMesh.checkIntegrity()");
		ctmMesh.checkIntegrity();

		Debug.Log("CTMReader.Read: BEGIN Converting OpenCTM.Mesh to UnityEngine.Mesh...");

		int verticesLength = ctmMesh.vertices.Length;
		//Debug.LogError("CTMReader.Read: ctmMesh.vertices.Length == " + verticesLength); // nox: 248145, brunnen: 2439
		//int numVertices = verticesLength / 3;
		//Debug.LogError("CTMReader.Read: numVertices == " + numVertices); // nox: ?, brunnen: ?
		List<Vector3> vertices = new List<Vector3>();
		for (int j = 0; j < verticesLength; j += 3)
		{
			vertices.Add(new Vector3(ctmMesh.vertices[j],
									 ctmMesh.vertices[j + 1],
									 ctmMesh.vertices[j + 2]));
		}
		//Debug.LogError("CTMReader.Read: vertices.Count == " + vertices.Count); // nox: ?, brunnen: ?

		bool hasNormals = ctmMesh.normals != null;
		//Debug.LogError("CTMReader.Read: hasNormals == " + hasNormals); // nox: False, brunnen: True
		List<Vector3> normals = new List<Vector3>();
		if (hasNormals)
		{
			for (int j = 0; j < verticesLength; j += 3)
			{
				normals.Add(new Vector3(ctmMesh.normals[j],
										ctmMesh.normals[j + 1],
										ctmMesh.normals[j + 2]));
			}
		}
		//Debug.LogError("CTMReader.Read: normals.Count == " + normals.Count); // nox: ?, brunnen: ?

		int indicesLength = ctmMesh.indices.Length;
		//Debug.LogError("CTMReader.Read: ctmMesh.indices.Length == " + indicesLength); // nox: 437886, brunnen: 4329
		int numTriangles = indicesLength / 3;
		Debug.LogWarning("CTMReader.Read: numTriangles == " + numTriangles); // nox: ?, brunnen: ?

		if (false)
		{
			int[] triangles = ctmMesh.indices.Clone() as int[];
			Debug.LogError("CTMReader.Read: triangles.Length == " + triangles.Length); // nox: ?, brunnen: ?

			List<Vector2> uv = new List<Vector2>();
			/*
            // TODO:(pv) Texture...
            //Debug.LogError("CTMReader.Read: ctmMesh.texcoordinates == " + ctmMesh.texcoordinates);
            if (ctmMesh.texcoordinates.Length > 0)
            {
                for (int j = 0; j < ctmMesh.texcoordinates[0].values.Length / 2; j++)
                {
                    uv.Add(new Vector2(ctmMesh.texcoordinates[0].values[(j * 2)],
                                       ctmMesh.texcoordinates[0].values[(j * 2) + 1]));
                }
            }
            */
			Debug.LogError("CTMReader.Read: uv.Count == " + uv.Count); // nox: ?, brunnen: ?

			Debug.Log("CTMReader.Read: unityMesh = new UnityEngine.Mesh(...)");
			UnityEngine.Mesh unityMesh = new UnityEngine.Mesh()
			{
				vertices = vertices.ToArray(),
				triangles = triangles,
				normals = normals.ToArray(),
				uv = uv.ToArray()
			};

			Debug.Log("CTMReader.Read: unityMesh.RecalculateBounds()");
			unityMesh.RecalculateBounds();
			Debug.Log("CTMReader.Read: unityMesh.RecalculateNormals()");
			unityMesh.RecalculateNormals();

			MeshRenderer mr = root.AddComponent<MeshRenderer>();
			Material material = new Material(Shader.Find("Standard"));
			mr.material = material;
			MeshFilter mf = root.AddComponent<MeshFilter>();
			mf.mesh = unityMesh;
		}
		else if (true)
		{
			//List<Vector2> uv = new List<Vector2>();
			/*
            // TODO:(pv) Texture...
            //Debug.LogError("CTMReader.Read: ctmMesh.texcoordinates == " + ctmMesh.texcoordinates);
            if (ctmMesh.texcoordinates.Length > 0)
            {
                for (int j = 0; j < ctmMesh.texcoordinates[0].values.Length / 2; j++)
                {
                    uv.Add(new Vector2(ctmMesh.texcoordinates[0].values[(j * 2)],
                                       ctmMesh.texcoordinates[0].values[(j * 2) + 1]));
                }
            }
            */
			//Debug.LogError("CTMReader.Read: uv.Count == " + uv.Count); // nox: ?, brunnen: ?

			int meshCount = numTriangles / CTMReader.MAX_TRIANGLES_PER_MESH + 1;
			Debug.Log("CTMReader.Read: meshCount == " + meshCount);
			MeshInfo[] meshInfos = new MeshInfo[meshCount];

			//
			// Walk the triangles, pulling in the appropriate vertices, normals, uvs
			//
			int meshIndex;
			MeshInfo meshInfo;
			int indicesIndex;
			int triangleVertex1, triangleVertex2, triangleVertex3;
			Vector3 vertex1, vertex2, vertex3;
			Vector3 normal1, normal2, normal3;
			int triangleVertexMax = 0;
			int triangleVertexNew = triangleVertexMax;
			for (int triangleIndex = 0; triangleIndex < numTriangles; triangleIndex++)
			{
				//Debug.LogError("CTMReader.Read: triangleIndex == " + triangleIndex);

				meshIndex = triangleIndex / CTMReader.MAX_TRIANGLES_PER_MESH;
				//Debug.LogError("CTMReader.Read: meshIndex == " + meshIndex);

				meshInfo = meshInfos[meshIndex];
				if (meshInfo == null)
				{
					meshInfo = new MeshInfo();
					meshInfos[meshIndex] = meshInfo;
				}

				indicesIndex = triangleIndex * 3;
				//Debug.LogError("CTMReader.Read: indicesIndex == " + indicesIndex);

				triangleVertex1 = ctmMesh.indices[indicesIndex];
				//Debug.LogError("CTMReader.Read: triangleVertex1 == " + triangleVertex1);
				meshInfo.triangles.Add(meshInfo.vertices.Count);
				vertex1 = vertices[triangleVertex1];
				//Debug.LogError("CTMReader.Read: vertex1 == " + vertex1);
				meshInfo.vertices.Add(vertex1);
				if (hasNormals)
				{
					normal1 = normals[triangleVertex1];
					//Debug.LogError("CTMReader.Read: normal1 == " + normal1);
					meshInfo.normals.Add(normal1);
				}

				triangleVertex2 = ctmMesh.indices[indicesIndex + 1];
				//Debug.LogError("CTMReader.Read: triangleVertex2 == " + triangleVertex2);
				meshInfo.triangles.Add(meshInfo.vertices.Count);
				vertex2 = vertices[triangleVertex2];
				//Debug.LogError("CTMReader.Read: vertex2 == " + vertex2);
				meshInfo.vertices.Add(vertex2);
				if (hasNormals)
				{
					normal2 = normals[triangleVertex2];
					//Debug.LogError("CTMReader.Read: normal2 == " + normal2);
					meshInfo.normals.Add(normal2);
				}

				triangleVertex3 = ctmMesh.indices[indicesIndex + 2];
				//Debug.LogError("CTMReader.Read: triangleVertex3 == " + triangleVertex3);
				meshInfo.triangles.Add(meshInfo.vertices.Count);
				vertex3 = vertices[triangleVertex3];
				//Debug.LogError("CTMReader.Read: vertex3 == " + vertex3);
				meshInfo.vertices.Add(vertex3);
				if (hasNormals)
				{
					normal3 = normals[triangleVertex3];
					//Debug.LogError("CTMReader.Read: normal3 == " + normal3);
					meshInfo.normals.Add(normal3);
				}

				if (triangleVertex1 > triangleVertexMax)
				{
					triangleVertexNew = triangleVertex1;
				}
				else if (triangleVertex2 > triangleVertexMax)
				{
					triangleVertexNew = triangleVertex2;
				}
				else if (triangleVertex3 > triangleVertexMax)
				{
					triangleVertexNew = triangleVertex3;
				}
				if (triangleVertexNew > triangleVertexMax)
				{
					triangleVertexMax = triangleVertexNew;
					//Debug.LogError("CTMReader.Read: triangleVertexMax == " + triangleVertexMax);
				}
			}

			Debug.Log("CTMReader.Read: Creating Unity Mesh(es)");
			GameObject child;
			UnityEngine.Mesh unityMesh;
			for (meshIndex = 0; meshIndex < meshCount; meshIndex++)
			{
				//Debug.LogError("CTMReader.Read: meshIndex == " + meshIndex);

				meshInfo = meshInfos[meshIndex];
				//Debug.LogError("CTMReader.Read: meshInfo.vertices.Count == " + meshInfo.vertices.Count);
				//Debug.LogError("CTMReader.Read: meshInfo.normals.Count == " + meshInfo.normals.Count);
				//Debug.LogError("CTMReader.Read: meshInfo.triangles.Count == " + meshInfo.triangles.Count);
				//Debug.LogError("CTMReader.Read: meshInfo.uv.Count == " + meshInfo.uv.Count);

				//Debug.Log("CTMReader.Read: unityMesh = new UnityEngine.Mesh(...)");
				unityMesh = new UnityEngine.Mesh()
				{
					vertices = meshInfo.vertices.ToArray(),
					triangles = meshInfo.triangles.ToArray(),
					normals = meshInfo.normals.ToArray(),
					uv = meshInfo.uv.ToArray()
				};

				//Debug.Log("CTMReader.Read: unityMesh.RecalculateBounds()");
				unityMesh.RecalculateBounds();
				//Debug.Log("CTMReader.Read: unityMesh.RecalculateNormals()");
				unityMesh.RecalculateNormals();

				child = new GameObject();
				MeshRenderer mr = child.AddComponent<MeshRenderer>();
				//Shader shader = Shader.Find("Standard");
				Shader shader = Shader.Find("Projector/Light");
				Material material = new Material(shader);
				mr.material = material;
				MeshFilter mf = child.AddComponent<MeshFilter>();
				mf.mesh = unityMesh;

				child.transform.SetParent(root.transform);
			}
		}

		Debug.Log("CTMReader.Read: END Converting OpenCTM.Mesh to UnityEngine.Mesh");

		return root;
	}

	private static Bounds CalculateBounds(GameObject go)
	{
		Bounds bounds;

		Transform transform = go.transform;
		Quaternion currentRotation = transform.rotation;
		{
			transform.rotation = Quaternion.Euler(0f, 0f, 0f);

			bounds = new Bounds(transform.position, Vector3.zero);
			foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
			{
				bounds.Encapsulate(renderer.bounds);
			}

		}
		transform.rotation = currentRotation;

		return bounds;
	}
}
