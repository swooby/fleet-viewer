using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using OpenCTM;

public class ModelsManager : MonoBehaviour
{
    class Systems
    {
        public const string StarCitizen = "StarCitizen";
    }

    // TODO:(pv) Load this from a[nother] configuration file...
    Dictionary<string, string> systems = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { Systems.StarCitizen, "Star Citizen Ships 3D Models - Data" },
    };

    private string systemName;

    public string SystemName
    {
        get
        {
            return systemName;
        }
        set
        {
            Configure(value);
        }
    }

    public SortedDictionary<string, ModelInfo> ModelInfos { get; private set; }

    void Start()
    {
        if (true)
        {
            // TODO:(pv) Load/restore the previously loaded game system; for now this is hardcoded
            SystemName = Systems.StarCitizen;
        }
        else if (true)
        {
            string resourcePath = "brunnen.ctm";
            LoadCTM(resourcePath);
        }
        else if (true)
        {
            if (false)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                /*
                for (int y = 0; y < 5; y++)
                {
                    for (int x = 0; x < 5; x++)
                    {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.AddComponent<Rigidbody>();
                        cube.transform.position = new Vector3(x, y, 0);
                    }
                }
                */
            }
            else
            {
                GameObject root = new GameObject();

                int unit = 5;

                for (int i = 0; i < 5; i++)
                {
                    MeshInfo meshInfo1 = new MeshInfo();
                    meshInfo1.vertices.Add(new Vector3(0, 0, unit * (i + 1)));
                    meshInfo1.vertices.Add(new Vector3(unit, 0, unit * (i + 1)));
                    meshInfo1.vertices.Add(new Vector3(0, unit, unit * (i + 1)));
                    meshInfo1.vertices.Add(new Vector3(unit, unit, unit * (i + 1)));

                    meshInfo1.triangles.Add(0);
                    meshInfo1.triangles.Add(2);
                    meshInfo1.triangles.Add(1);
                    meshInfo1.triangles.Add(2);
                    meshInfo1.triangles.Add(3);
                    meshInfo1.triangles.Add(1);

                    /*
                    meshInfo1.normals.Add(-Vector3.forward);
                    meshInfo1.normals.Add(-Vector3.forward);
                    meshInfo1.normals.Add(-Vector3.forward);
                    meshInfo1.normals.Add(-Vector3.forward);
                    */

                    /*
                    meshInfo1.uv.Add(new Vector2(0, 0));
                    meshInfo1.uv.Add(new Vector2(1, 0));
                    meshInfo1.uv.Add(new Vector2(0, 1));
                    meshInfo1.uv.Add(new Vector2(1, 1));
                    */

                    UnityEngine.Mesh mesh = new UnityEngine.Mesh()
                    {
                        vertices = meshInfo1.vertices.ToArray(),
                        triangles = meshInfo1.triangles.ToArray(),
                        normals = meshInfo1.normals.ToArray(),
                        uv = meshInfo1.uv.ToArray(),
                    };

                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();

                    GameObject child = new GameObject();
                    MeshRenderer mr = child.AddComponent<MeshRenderer>();
                    Material material = new Material(Shader.Find("Standard"));
                    mr.material = material;
                    MeshFilter mf = child.AddComponent<MeshFilter>();
                    mf.mesh = mesh;

                    child.transform.SetParent(root.transform);
                }
            }
        }
    }

    private void Configure(string systemName)
    {
        string configurationFilePath;
        if (!systems.TryGetValue(systemName, out configurationFilePath))
        {
            Debug.LogError("Configure: Failed to load systemName == \"" + systemName + "\"");
            return;
        }

        this.systemName = systemName;

        LoadModelInfos(configurationFilePath);

        // TODO:(pv) Load/restore previously loaded models; for now this is hardcoded
        string modelName = "Nox";
        //string modelName = "Starfarer";
        //string modelName = "Idris-P";
        LoadScalePositionModel(modelName);
    }

    public void LoadModelInfos(string configurationFilePath)
    {
        ModelInfos = new SortedDictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);
        CSVReader.Read<ModelInfo>(configurationFilePath, (dictionary) =>
        {
            ModelInfo modelInfo = new ModelInfo(dictionary);
            ModelInfos[modelInfo.Name] = modelInfo;
            return null;
        });
        Debug.Log("LoadModelInfos: ModelInfos.Count == " + ModelInfos.Count);

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
    }

    private GameObject LoadScalePositionModel(string modelName)
    {
        ModelInfo modelInfo;
        if (!ModelInfos.TryGetValue(modelName, out modelInfo) || modelInfo == null)
        {
            Debug.LogError("LoadScalePositionModel: Failed to load modelName == \"" + modelName + "\"");
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

        // TODO:(pv) Auto-arrange/position according to scale and previously loaded models...

        return go;
    }

    private Bounds CalculateBounds(GameObject go)
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

    private const int MAX_VERTICES_PER_MESH = 65000;
    private const int MAX_TRIANGLES_PER_MESH = ((65000 / 3) * 3) / 3; // 64998 / 3 == 21666

    class MeshInfo
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<Vector2> uv = new List<Vector2>();
    }

    // TODO:(pv) Make this [or upstream caller] an async task so that we don't block...
    //  https://www.google.com/search?q=unity+async+load
    private GameObject LoadCTM(string resourcePath)
    {
        Debug.Log("LoadCTM(resourcePath:" + Utils.Quote(resourcePath) + ")");

        return CTMReader.Read(resourcePath);

        Debug.LogWarning("LoadCTM: MAX_TRIANGLES_PER_MESH == " + MAX_TRIANGLES_PER_MESH);

        GameObject root = new GameObject();

        TextAsset textAsset = Resources.Load(resourcePath) as TextAsset;
        Stream memoryStream = new MemoryStream(textAsset.bytes);

        //Debug.Log("LoadCTM: new CtmFileReader(memoryStream)");
        CtmFileReader reader = new CtmFileReader(memoryStream);
        //Debug.Log("LoadCTM: reader.decode()");
        OpenCTM.Mesh ctmMesh = reader.decode();
        //Debug.Log("LoadCTM: ctmMesh.checkIntegrity()");
        ctmMesh.checkIntegrity();

        Debug.Log("LoadCTM: BEGIN Converting OpenCTM.Mesh to UnityEngine.Mesh...");

        int verticesLength = ctmMesh.vertices.Length;
        //Debug.LogError("LoadCTM: ctmMesh.vertices.Length == " + verticesLength); // nox: 248145, brunnen: 2439
        //int numVertices = verticesLength / 3;
        //Debug.LogError("LoadCTM: numVertices == " + numVertices); // nox: ?, brunnen: ?
        List<Vector3> vertices = new List<Vector3>();
        for (int j = 0; j < verticesLength; j += 3)
        {
            vertices.Add(new Vector3(ctmMesh.vertices[j],
                                     ctmMesh.vertices[j + 1],
                                     ctmMesh.vertices[j + 2]));
        }
        //Debug.LogError("LoadCTM: vertices.Count == " + vertices.Count); // nox: ?, brunnen: ?

        bool hasNormals = ctmMesh.normals != null;
        //Debug.LogError("LoadCTM: hasNormals == " + hasNormals); // nox: False, brunnen: True
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
        //Debug.LogError("LoadCTM: normals.Count == " + normals.Count); // nox: ?, brunnen: ?

        int indicesLength = ctmMesh.indices.Length;
        //Debug.LogError("LoadCTM: ctmMesh.indices.Length == " + indicesLength); // nox: 437886, brunnen: 4329
        int numTriangles = indicesLength / 3;
        Debug.LogWarning("LoadCTM: numTriangles == " + numTriangles); // nox: ?, brunnen: ?

        if (false)
        {
            int[] triangles = ctmMesh.indices.Clone() as int[];
            Debug.LogError("LoadCTM: triangles.Length == " + triangles.Length); // nox: ?, brunnen: ?

            List<Vector2> uv = new List<Vector2>();
            /*
            // TODO:(pv) Texture...
            //Debug.LogError("LoadCTM: ctmMesh.texcoordinates == " + ctmMesh.texcoordinates);
            if (ctmMesh.texcoordinates.Length > 0)
            {
                for (int j = 0; j < ctmMesh.texcoordinates[0].values.Length / 2; j++)
                {
                    uv.Add(new Vector2(ctmMesh.texcoordinates[0].values[(j * 2)],
                                       ctmMesh.texcoordinates[0].values[(j * 2) + 1]));
                }
            }
            */
            Debug.LogError("LoadCTM: uv.Count == " + uv.Count); // nox: ?, brunnen: ?

            Debug.Log("LoadCTM: unityMesh = new UnityEngine.Mesh(...)");
            UnityEngine.Mesh unityMesh = new UnityEngine.Mesh()
            {
                vertices = vertices.ToArray(),
                triangles = triangles,
                normals = normals.ToArray(),
                uv = uv.ToArray()
            };

            Debug.Log("LoadCTM: unityMesh.RecalculateBounds()");
            unityMesh.RecalculateBounds();
            Debug.Log("LoadCTM: unityMesh.RecalculateNormals()");
            unityMesh.RecalculateNormals();

            MeshRenderer mr = root.AddComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("Standard"));
            mr.material = material;
            MeshFilter mf = root.AddComponent<MeshFilter>();
            mf.mesh = unityMesh;
        }
        else if (true)
        {
            List<Vector2> uv = new List<Vector2>();
            /*
			// TODO:(pv) Texture...
			//Debug.LogError("LoadCTM: ctmMesh.texcoordinates == " + ctmMesh.texcoordinates);
			if (ctmMesh.texcoordinates.Length > 0)
			{
				for (int j = 0; j < ctmMesh.texcoordinates[0].values.Length / 2; j++)
				{
					uv.Add(new Vector2(ctmMesh.texcoordinates[0].values[(j * 2)],
									   ctmMesh.texcoordinates[0].values[(j * 2) + 1]));
				}
			}
            */
            //Debug.LogError("LoadCTM: uv.Count == " + uv.Count); // nox: ?, brunnen: ?

            int meshCount = numTriangles / MAX_TRIANGLES_PER_MESH + 1;
            Debug.Log("LoadCTM: meshCount == " + meshCount);
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
                //Debug.LogError("LoadCTM: triangleIndex == " + triangleIndex);

                meshIndex = triangleIndex / MAX_TRIANGLES_PER_MESH;
                //Debug.LogError("LoadCTM: meshIndex == " + meshIndex);

                meshInfo = meshInfos[meshIndex];
                if (meshInfo == null)
                {
                    meshInfo = new MeshInfo();
                    meshInfos[meshIndex] = meshInfo;
                }

                indicesIndex = triangleIndex * 3;
                //Debug.LogError("LoadCTM: indicesIndex == " + indicesIndex);

                triangleVertex1 = ctmMesh.indices[indicesIndex];
                //Debug.LogError("LoadCTM: triangleVertex1 == " + triangleVertex1);
                meshInfo.triangles.Add(meshInfo.vertices.Count);
                vertex1 = vertices[triangleVertex1];
                //Debug.LogError("LoadCTM: vertex1 == " + vertex1);
                meshInfo.vertices.Add(vertex1);
                if (hasNormals)
                {
                    normal1 = normals[triangleVertex1];
                    //Debug.LogError("LoadCTM: normal1 == " + normal1);
                    meshInfo.normals.Add(normal1);
                }

                triangleVertex2 = ctmMesh.indices[indicesIndex + 1];
                //Debug.LogError("LoadCTM: triangleVertex2 == " + triangleVertex2);
                meshInfo.triangles.Add(meshInfo.vertices.Count);
                vertex2 = vertices[triangleVertex2];
                //Debug.LogError("LoadCTM: vertex2 == " + vertex2);
                meshInfo.vertices.Add(vertex2);
                if (hasNormals)
                {
                    normal2 = normals[triangleVertex2];
                    //Debug.LogError("LoadCTM: normal2 == " + normal2);
                    meshInfo.normals.Add(normal2);
                }

                triangleVertex3 = ctmMesh.indices[indicesIndex + 2];
                //Debug.LogError("LoadCTM: triangleVertex3 == " + triangleVertex3);
                meshInfo.triangles.Add(meshInfo.vertices.Count);
                vertex3 = vertices[triangleVertex3];
                //Debug.LogError("LoadCTM: vertex3 == " + vertex3);
                meshInfo.vertices.Add(vertex3);
                if (hasNormals)
                {
                    normal3 = normals[triangleVertex3];
                    //Debug.LogError("LoadCTM: normal3 == " + normal3);
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
                    //Debug.LogError("LoadCTM: triangleVertexMax == " + triangleVertexMax);
                }
            }

            Debug.Log("LoadCTM: Creating Unity Mesh(es)");
            GameObject child;
            UnityEngine.Mesh unityMesh;
            for (meshIndex = 0; meshIndex < meshCount; meshIndex++)
            {
                //Debug.LogError("LoadCTM: meshIndex == " + meshIndex);

                meshInfo = meshInfos[meshIndex];
                //Debug.LogError("LoadCTM: meshInfo.vertices.Count == " + meshInfo.vertices.Count);
                //Debug.LogError("LoadCTM: meshInfo.normals.Count == " + meshInfo.normals.Count);
                //Debug.LogError("LoadCTM: meshInfo.triangles.Count == " + meshInfo.triangles.Count);
                //Debug.LogError("LoadCTM: meshInfo.uv.Count == " + meshInfo.uv.Count);

                //Debug.Log("LoadCTM: unityMesh = new UnityEngine.Mesh(...)");
                unityMesh = new UnityEngine.Mesh()
                {
                    vertices = meshInfo.vertices.ToArray(),
                    triangles = meshInfo.triangles.ToArray(),
                    normals = meshInfo.normals.ToArray(),
                    uv = meshInfo.uv.ToArray()
                };

                //Debug.Log("LoadCTM: unityMesh.RecalculateBounds()");
                unityMesh.RecalculateBounds();
                //Debug.Log("LoadCTM: unityMesh.RecalculateNormals()");
                unityMesh.RecalculateNormals();

                child = new GameObject();
                MeshRenderer mr = child.AddComponent<MeshRenderer>();
                Shader shader = Shader.Find("Standard");
                Material material = new Material(shader);
                mr.material = material;
                MeshFilter mf = child.AddComponent<MeshFilter>();
                mf.mesh = unityMesh;

                child.transform.SetParent(root.transform);
            }
        }

        Debug.Log("LoadCTM: END Converting OpenCTM.Mesh to UnityEngine.Mesh");

        return root;
    }
}
