#if UNITY_ANDROID && !UNITY_EDITOR
#define RUNNING_ON_ANDROID_DEVICE
#endif  // UNITY_ANDROID && !UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using OpenCTM;
using System;
using System.Runtime.Remoting.Messaging;

public class CTMReader
{
    public const bool VERBOSE_LOG = false;

    public const int MAX_VERTICES_PER_MESH = 65000;
    public const int MAX_TRIANGLES_PER_MESH = ((65000 / 3) * 3) / 3; // 64998 / 3 == 21666

    private class MeshInfo
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<Vector2> uv = new List<Vector2>();

        private UnityEngine.Mesh mesh;

        public UnityEngine.Mesh Mesh
        {
            get
            {
                if (mesh == null)
                {
                    //Debug.Log("MeshInfo: mesh = new UnityEngine.Mesh(...)");
                    mesh = new UnityEngine.Mesh()
                    {
                        vertices = vertices.ToArray(),
                        triangles = triangles.ToArray(),
                        normals = normals.ToArray(),
                        uv = uv.ToArray()
                    };
                    //Debug.Log("MeshInfo: mesh.RecalculateBounds()");
                    mesh.RecalculateBounds();
                    //Debug.Log("MeshInfo: mesh.RecalculateNormals()");
                    mesh.RecalculateNormals();

                    if (false)
                    {
                        //
                        // NOTE:(pv) This seems to be causing GL_OUT_OF_MEMORY error
                        //
                        //Debug.LogWarning("MeshInfo: mesh.RecalculateTangents()");
                        mesh.RecalculateTangents();
                    }
                }
                return mesh;
            }
        }
    }

    private CTMReader()
    {
    }

    public static GameObject Load(string resourcePath)
    {
        Debug.Log("CTMReader.Load(resourcePath:" + Utils.Quote(resourcePath) + ")");

        byte[] bytes = GetBytes(resourcePath);

        MeshInfo[] meshInfos = GetMeshInfos(bytes);

        GameObject gameObject = meshInfos != null ? GetGameObject(meshInfos) : null;

        return gameObject;
    }

    public delegate void LoadCallback(GameObject gameObject);

    private delegate MeshInfo[] GetMeshInfosCaller(byte[] bytes);

    public static void LoadAsync(string resourcePath, LoadCallback callback)
    {
        Debug.Log("CTMReader.LoadAsync(resourcePath:" + Utils.Quote(resourcePath) +
                  ", callback:" + callback + ")");

        byte[] bytes = GetBytes(resourcePath);

        IAsyncResult asyncResult = new GetMeshInfosCaller(GetMeshInfos)
            .BeginInvoke(bytes, OnGetMeshInfos, callback);

        if (!asyncResult.CompletedSynchronously)
        {
            return;
        }

        OnGetMeshInfos(asyncResult);
    }

    private static void OnGetMeshInfos(IAsyncResult asyncResult)
    {
        AsyncResult result = (AsyncResult)asyncResult;

        GetMeshInfosCaller caller = (GetMeshInfosCaller)result.AsyncDelegate;

        MeshInfo[] meshInfos = caller.EndInvoke(asyncResult);

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameObject gameObject = meshInfos != null ? GetGameObject(meshInfos) : null;

            LoadCallback callback = (LoadCallback)asyncResult.AsyncState;

            callback(gameObject);
        });
    }

    private static byte[] GetBytes(string resourcePath)
    {
        TextAsset textAsset = Resources.Load(resourcePath) as TextAsset;
        byte[] bytes = textAsset.bytes;
        return bytes;
    }

    private static MeshInfo[] GetMeshInfos(byte[] bytes)
    {
        if (VERBOSE_LOG)
        {
            Debug.LogWarning("CTMReader.GetMeshInfos: MAX_TRIANGLES_PER_MESH == " + MAX_TRIANGLES_PER_MESH);
        }

        Stream memoryStream = new MemoryStream(bytes);

        //Debug.Log("CTMReader.GetMeshInfos: new CtmFileReader(memoryStream)");
        CtmFileReader reader = new CtmFileReader(memoryStream);
        //Debug.Log("CTMReader.GetMeshInfos: reader.decode()");
        OpenCTM.Mesh ctmMesh = reader.decode();
        //Debug.Log("CTMReader.GetMeshInfos: ctmMesh.checkIntegrity()");
        ctmMesh.checkIntegrity();

        if (VERBOSE_LOG)
        {
            Debug.LogWarning("CTMReader.GetMeshInfos: BEGIN Converting OpenCTM.Mesh to UnityEngine.Mesh...");
        }

        int verticesLength = ctmMesh.vertices.Length;
        //Debug.LogError("CTMReader.GetMeshInfos: ctmMesh.vertices.Length == " + verticesLength);
        //int numVectors = verticesLength / 3;
        //Debug.LogError("CTMReader.GetMeshInfos: numVectors == " + numVectors);
        List<Vector3> vertices = new List<Vector3>();
        for (int j = 0; j < verticesLength; j += 3)
        {
            vertices.Add(new Vector3(ctmMesh.vertices[j],
                                     ctmMesh.vertices[j + 1],
                                     ctmMesh.vertices[j + 2]));
        }
        //Debug.LogError("CTMReader.GetMeshInfos: vertices.Count == " + vertices.Count);

        bool hasNormals = ctmMesh.normals != null;
        //Debug.LogError("CTMReader.GetMeshInfos: hasNormals == " + hasNormals);
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
        //Debug.LogError("CTMReader.GetMeshInfos: normals.Count == " + normals.Count);

        int indicesLength = ctmMesh.indices.Length;
        //Debug.LogError("CTMReader.GetMeshInfos: ctmMesh.indices.Length == " + indicesLength);
        int numTriangles = indicesLength / 3;
        if (VERBOSE_LOG)
        {
            Debug.LogWarning("CTMReader.GetMeshInfos: numTriangles == " + numTriangles);
        }

        //List<Vector2> uv = new List<Vector2>();
        /*
        // TODO:(pv) Texture...
        //Debug.LogError("CTMReader.GetMeshInfos: ctmMesh.texcoordinates == " + ctmMesh.texcoordinates);
        if (ctmMesh.texcoordinates.Length > 0)
        {
            for (int j = 0; j < ctmMesh.texcoordinates[0].values.Length / 2; j++)
            {
                uv.Add(new Vector2(ctmMesh.texcoordinates[0].values[(j * 2)],
                                   ctmMesh.texcoordinates[0].values[(j * 2) + 1]));
            }
        }
        */
        //Debug.LogError("CTMReader.GetMeshInfos: uv.Count == " + uv.Count);

        int meshCount = (int)Math.Ceiling((double)(numTriangles / (double)MAX_TRIANGLES_PER_MESH));
        if (VERBOSE_LOG)
        {
            Debug.LogWarning("CTMReader.GetMeshInfos: meshCount == " + meshCount);
        }
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

        int debugTriangleIndex = int.MaxValue;

        for (int triangleIndex = 0; triangleIndex < numTriangles; triangleIndex++)
        {
            if (triangleIndex >= debugTriangleIndex)
            {
                Debug.LogError("CTMReader.GetMeshInfos: triangleIndex == " + triangleIndex);
            }

            meshIndex = triangleIndex / MAX_TRIANGLES_PER_MESH;
            if (triangleIndex >= debugTriangleIndex)
            {
                Debug.LogError("CTMReader.GetMeshInfos: meshIndex == " + meshIndex);
            }

            meshInfo = meshInfos[meshIndex];
            if (meshInfo == null)
            {
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError("CTMReader.GetMeshInfos: meshInfos[" + meshIndex + "] = new MeshInfo();");
                }
                meshInfo = new MeshInfo();
                meshInfos[meshIndex] = meshInfo;
            }

            if (triangleIndex >= debugTriangleIndex)
            {
                Debug.LogError("CTMReader.GetMeshInfos: meshInfo == " + meshInfo);
            }

            indicesIndex = triangleIndex * 3;
            if (triangleIndex >= debugTriangleIndex)
            {
                Debug.LogError("CTMReader.GetMeshInfos: indicesIndex == " + indicesIndex);
            }

            triangleVertex1 = ctmMesh.indices[indicesIndex];
            if (triangleIndex >= debugTriangleIndex)
            {
                Debug.LogError("CTMReader.GetMeshInfos: triangleVertex1 == " + triangleVertex1);
            }
            meshInfo.triangles.Add(meshInfo.vertices.Count);
            vertex1 = vertices[triangleVertex1];
            if (triangleIndex >= debugTriangleIndex)
            {
                Debug.LogError("CTMReader.GetMeshInfos: vertex1 == " + vertex1);
            }
            meshInfo.vertices.Add(vertex1);
            if (hasNormals)
            {
                normal1 = normals[triangleVertex1];
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError("CTMReader.GetMeshInfos: normal1 == " + normal1);
                }
                meshInfo.normals.Add(normal1);
            }

            triangleVertex2 = ctmMesh.indices[indicesIndex + 1];
            if (triangleIndex >= debugTriangleIndex)
            {
                Debug.LogError("CTMReader.GetMeshInfos: triangleVertex2 == " + triangleVertex2);
            }
            meshInfo.triangles.Add(meshInfo.vertices.Count);
            vertex2 = vertices[triangleVertex2];
            if (triangleIndex >= debugTriangleIndex)
            {
                Debug.LogError("CTMReader.GetMeshInfos: vertex2 == " + vertex2);
            }
            meshInfo.vertices.Add(vertex2);
            if (hasNormals)
            {
                normal2 = normals[triangleVertex2];
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError("CTMReader.GetMeshInfos: normal2 == " + normal2);
                }
                meshInfo.normals.Add(normal2);
            }

            triangleVertex3 = ctmMesh.indices[indicesIndex + 2];
            if (triangleIndex >= debugTriangleIndex)
            {
                Debug.LogError("CTMReader.GetMeshInfos: triangleVertex3 == " + triangleVertex3);
            }
            meshInfo.triangles.Add(meshInfo.vertices.Count);
            vertex3 = vertices[triangleVertex3];
            if (triangleIndex >= debugTriangleIndex)
            {
                Debug.LogError("CTMReader.GetMeshInfos: vertex3 == " + vertex3);
            }
            meshInfo.vertices.Add(vertex3);
            if (hasNormals)
            {
                normal3 = normals[triangleVertex3];
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError("CTMReader.GetMeshInfos: normal3 == " + normal3);
                }
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
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError("CTMReader.GetMeshInfos: triangleVertexMax == " + triangleVertexMax);
                }
            }
        }

        return meshInfos;
    }

    private static GameObject GetGameObject(MeshInfo[] meshInfos)
    {
        if (meshInfos == null)
        {
            return null;
        }

        GameObject root = new GameObject();

        if (VERBOSE_LOG)
        {
            Debug.LogWarning("CTMReader.GetGameObject: Creating Unity Mesh(es)");
        }

        int meshCount = meshInfos.Length;
        MeshInfo meshInfo;
        UnityEngine.Mesh unityMesh;
        GameObject child;

        int debugMeshIndex = int.MaxValue;

        for (int meshIndex = 0; meshIndex < meshCount; meshIndex++)
        {
            if (meshIndex >= debugMeshIndex)
            {
                Debug.LogError("CTMReader.GetGameObject: meshIndex == " + meshIndex);
            }

            meshInfo = meshInfos[meshIndex];
            if (meshIndex >= debugMeshIndex)
            {
                Debug.LogError("CTMReader.GetGameObject: meshInfo == " + meshInfo);
                Debug.LogError("CTMReader.GetGameObject: meshInfo.vertices.Count == " + meshInfo.vertices.Count);
                Debug.LogError("CTMReader.GetGameObject: meshInfo.normals.Count == " + meshInfo.normals.Count);
                Debug.LogError("CTMReader.GetGameObject: meshInfo.triangles.Count == " + meshInfo.triangles.Count);
                Debug.LogError("CTMReader.GetGameObject: meshInfo.uv.Count == " + meshInfo.uv.Count);
            }

            unityMesh = meshInfo.Mesh;

            child = new GameObject();
            child.name = "mesh" + meshIndex;

            MeshRenderer mr = child.AddComponent<MeshRenderer>();
#if RUNNING_ON_ANDROID_DEVICE
            //Shader shader = Shader.Find("Mobile/VertexLit (Only Directional Lights)");
            //Shader shader = Shader.Find("Mobile/VertexLit (Only Directional Lights) (Two Sided)");
            //Shader shader = Shader.Find("Mobile/VertexLit");
            //Shader shader = Shader.Find("Standard");
            Shader shader = Shader.Find("Standard (Two Sided)");
            //Shader shader = Shader.Find("Diffuse");
#else
            //Shader shader = Shader.Find("Mobile/VertexLit (Only Directional Lights) (Two Sided)");
            //Shader shader = Shader.Find("Standard");
            Shader shader = Shader.Find("Standard (Two Sided)");
            //Shader shader = Shader.Find("Diffuse");
            //Shader shader = Shader.Find("Projector/Light");
#endif
            mr.material = new Material(shader);

            MeshFilter mf = child.AddComponent<MeshFilter>();
            mf.mesh = unityMesh;

            child.transform.SetParent(root.transform);
        }

        if (VERBOSE_LOG)
        {
            Debug.LogWarning("CTMReader.GetGameObject: END Converting OpenCTM.Mesh to UnityEngine.Mesh");
        }

        return root;
    }
}