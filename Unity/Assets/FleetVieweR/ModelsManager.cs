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
    private const int MAX_TRIANGLES_PER_MESH = ((65000 / 3) * 3) / 3; // 64998
    private const int MAX_FACETS_PER_MESH = MAX_VERTICES_PER_MESH / 3;

    class MeshInfo
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<Vector2> uv = new List<Vector2>();
    }

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

        int verticesLength = ctmMesh.vertices.Length;
        Debug.LogError("LoadCTM: ctmMesh.vertices.Length == " + verticesLength); // nox: 248145, brunnen: 2439
        int numVertices = verticesLength / 3;
        Debug.LogError("LoadCTM: numVertices == " + numVertices); // nox: ?, brunnen: ?
        int indicesLength = ctmMesh.indices.Length;
        Debug.LogError("LoadCTM: ctmMesh.indices.Length == " + indicesLength); // nox: 437886, brunnen: 4329
        int numTriangles = indicesLength / 3;
        Debug.LogError("LoadCTM: numTriangles == " + numTriangles); // nox: ?, brunnen: ?
        bool hasNormals = ctmMesh.normals != null;
        Debug.LogError("LoadCTM: hasNormals == " + hasNormals); // nox: False, brunnen: True

        if (false)
        {
            List<Vector3> vertices = new List<Vector3>();
            for (int j = 0; j < verticesLength; j += 3)
            {
                vertices.Add(new Vector3(ctmMesh.vertices[j],
                                         ctmMesh.vertices[j + 1],
                                         ctmMesh.vertices[j + 2]));
            }
            Debug.LogError("LoadCTM: vertices.Count == " + vertices.Count); // nox: ?, brunnen: ?

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
            Debug.LogError("LoadCTM: normals.Count == " + normals.Count); // nox: ?, brunnen: ?

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

            GameObject go = new GameObject();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Diffuse"));
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = unityMesh;
        }
        else if (true)
        {
            List<Vector3> vertices = new List<Vector3>();
            for (int j = 0; j < verticesLength; j += 3)
            {
                vertices.Add(new Vector3(ctmMesh.vertices[j],
                                         ctmMesh.vertices[j + 1],
                                         ctmMesh.vertices[j + 2]));
            }
            Debug.LogError("LoadCTM: vertices.Count == " + vertices.Count); // nox: ?, brunnen: ?

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
            Debug.LogError("LoadCTM: normals.Count == " + normals.Count); // nox: ?, brunnen: ?

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

            int meshCount = numTriangles / MAX_TRIANGLES_PER_MESH + 1;
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
            UnityEngine.Mesh[] unityMeshes = new UnityEngine.Mesh[meshCount];
            UnityEngine.Mesh unityMesh;
            for (meshIndex = 0; meshIndex < meshCount; meshIndex++)
            {
                Debug.LogError("LoadCTM: meshIndex == " + meshIndex);

                meshInfo = meshInfos[meshIndex];
                Debug.LogError("LoadCTM: meshInfo.vertices.Count == " + meshInfo.vertices.Count);
                Debug.LogError("LoadCTM: meshInfo.normals.Count == " + meshInfo.normals.Count);
                Debug.LogError("LoadCTM: meshInfo.triangles.Count == " + meshInfo.triangles.Count);
                Debug.LogError("LoadCTM: meshInfo.uv.Count == " + meshInfo.uv.Count);

                Debug.Log("LoadCTM: unityMesh = new UnityEngine.Mesh(...)");
                unityMesh = new UnityEngine.Mesh()
                {
                    vertices = meshInfo.vertices.ToArray(),
                    triangles = meshInfo.triangles.ToArray(),
                    normals = meshInfo.normals.ToArray(),
                    uv = meshInfo.uv.ToArray()
                };

                Debug.Log("LoadCTM: unityMesh.RecalculateBounds()");
                unityMesh.RecalculateBounds();
                Debug.Log("LoadCTM: unityMesh.RecalculateNormals()");
                unityMesh.RecalculateNormals();

                GameObject go = new GameObject();
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.material = new Material(Shader.Find("Diffuse"));
                MeshFilter mf = go.AddComponent<MeshFilter>();
                mf.mesh = unityMesh;

                unityMeshes[meshIndex] = unityMesh;
            }
        }

        Debug.Log("LoadCTM: END Converting OpenCTM.Mesh to UnityEngine.Mesh");
    }
}
