using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using OpenCTM;

public class ModelsManager : MonoBehaviour
{
    void Start()
    {
        LoadModelInfos("Star Citizen/Star Citizen Ships 3D Models - Data");
        //LoadCTM("brunnen");
        //LoadCTM("Star Citizen/Nox/Xian_nox2");
        //LoadCTM("Star Citizen/Idris/Idris");
    }

    public SortedDictionary<String, ModelInfo> ModelInfos { get; private set; }

    public void LoadModelInfos(string path)
    {
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

        ModelInfos = new SortedDictionary<string, ModelInfo>();
        dropdown.ClearOptions();

        CSVReader.Read<ModelInfo>(path, dictionary =>
        {
            ModelInfo modelInfo = new ModelInfo(dictionary);
            ModelInfos[modelInfo.Name] = modelInfo;
            return null;
        });

        Debug.Log("LoadModelInfos: ModelInfos.Count == " + ModelInfos.Count);

        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        foreach (ModelInfo modelInfo in ModelInfos.Values)
        {
            if (modelInfo == null || modelInfo.HoloviewCtmUrl == null)
            {
                continue;
            }

            Dropdown.OptionData optionData = new Dropdown.OptionData()
            {
                text = modelInfo.Name
            };
            optionDatas.Add(optionData);
        }

        dropdown.AddOptions(optionDatas);
        dropdown.value = 0;

        Debug.Log("LoadModelInfos: dropdown.options.Count == " + dropdown.options.Count);
    }

    //private const int MAX_VERTICES_PER_MESH = 65535;
    private const int MAX_VERTICES_PER_MESH = 65000;
    private const int MAX_FACETS_PER_MESH = MAX_VERTICES_PER_MESH / 3;

    private void LoadCTM(string path)
    {
        Debug.Log("LoadCTM(\"" + path + "\")");

        FileStream file = new FileStream("Assets/Resources/" + path + ".ctm", FileMode.Open);

        //Debug.Log("LoadCTM: new CtmFileReader(file)");
        CtmFileReader reader = new CtmFileReader(file);
        //Debug.Log("LoadCTM: reader.decode()");
        OpenCTM.Mesh ctmMesh = reader.decode();
        //Debug.Log("LoadCTM: ctmMesh.checkIntegrity()");
        ctmMesh.checkIntegrity();

        Debug.Log("LoadCTM: BEGIN Converting OpenCTM.Mesh to UnityEngine.Mesh...");

        int vertexCount = ctmMesh.getVertexCount();
        Debug.LogError("LoadCTM: vertexCount == " + vertexCount);
        int indexCount = ctmMesh.indices.Length;
        Debug.LogError("LoadCTM: indexCount == " + indexCount);
        bool hasNormals = ctmMesh.hasNormals();
        Debug.LogError("LoadCTM: hasNormals == " + hasNormals);
        int normalCount = ctmMesh.normals.Length;
        Debug.LogError("LoadCTM: normalCount == " + normalCount);

        int numMeshes = vertexCount / MAX_FACETS_PER_MESH + 1;
        Debug.LogError("LoadCTM: numMeshes == " + numMeshes);
        UnityEngine.Mesh[] unityMeshes = new UnityEngine.Mesh[numMeshes];
        UnityEngine.Mesh unityMesh;

        for (int i = 0; i < unityMeshes.Length; i++)
        {
            if (true)
            {
                List<Vector3> vertices = new List<Vector3>();
                for (int j = 0; j < ctmMesh.getVertexCount(); j++)
                {
                    vertices.Add(new Vector3(ctmMesh.vertices[(j * 3)],
                                             ctmMesh.vertices[(j * 3) + 1],
                                             ctmMesh.vertices[(j * 3) + 2]));
                }

                List<Vector3> normals = new List<Vector3>();
                if (hasNormals)
                {
                    for (int j = 0; j < ctmMesh.normals.Length / 3; j++)
                    {
                        normals.Add(new Vector3(ctmMesh.normals[(j * 3)],
                                                ctmMesh.normals[(j * 3) + 1],
                                                ctmMesh.normals[(j * 3) + 2]));
                    }
                }

				// TODO:(pv) Texture...
				List<Vector2> uv = new List<Vector2>();


                Debug.Log("LoadCTM: unityMesh = new UnityEngine.Mesh(...)");
                unityMesh = new UnityEngine.Mesh()
                {
                    vertices = vertices.ToArray(),
                    triangles = ctmMesh.indices.Clone() as int[],
                    normals = normals.ToArray(),
                    uv = uv.ToArray()
                };
            }
            else
            {
            }

            Debug.Log("LoadCTM: unityMesh.RecalculateBounds()");
            unityMesh.RecalculateBounds();
            Debug.Log("LoadCTM: unityMesh.RecalculateNormals()");
            unityMesh.RecalculateNormals();

            Debug.Log("LoadCTM: END Converting OpenCTM.Mesh to UnityEngine.Mesh");

            GameObject go = new GameObject();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Diffuse"));
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = unityMesh;

            unityMeshes[i] = unityMesh;
        }
    }
}
