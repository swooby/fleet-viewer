#if UNITY_ANDROID && !UNITY_EDITOR
#define RUNNING_ON_ANDROID_DEVICE
#endif  // UNITY_ANDROID && !UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using OpenCTM;
using System;
using System.Runtime.Remoting.Messaging;

// TODO:(pv) Make this a plugin in the Unity Store
//  https://unity3d.com/learn/tutorials/topics/scripting/writing-plugins
//  https://docs.unity3d.com/Manual/Plugins.html
//  https://www.youtube.com/watch?v=uFjiNkYhBvY
//  https://www.google.com/search?q=how+to+make+a+unity+plugin

namespace FleetVieweR
{
    /// <summary>
    /// Original inspiration from several sources:
    ///     https://github.com/Danny02/JOpenCTM
    ///     https://github.com/Danny02/OpenCtm-CSharp
    ///     https://github.com/unity-car-tutorials/OpenCTM-Unity
    ///     https://github.com/unity-car-tutorials/OpenCTM-Unity
    /// 
    /// CTMReader loads a single CTM file in to a UnityEngine.Mesh.
    /// This implementation does not fail if the CTM has more than the Unity limit
    /// of 65K vertices or triangles per UnityEngine.Mesh.
    /// There is confusion on my part if UnityEngine.Mesh is limited to 65K vertices
    /// or triangles per mesh, so I assume the worst case of 65k vertices per mesh
    /// and assume 3 vertices per triangle to come up with a working limit of 21666
    /// triangles per UnityEngine.Mesh.
    /// In this implementation, separate UnityEngine.Mesh will be create every
    /// 21666 triangles, and all will be grouped to have the same root/parent.
    /// 
    /// NOTE: Normals and Textures are not yet supported.
    /// </summary>
    public class CTMReader
    {
        private static readonly string TAG = Utils.TAG<CTMReader>();

        public const bool VERBOSE_LOG = false;

        public const int MAX_VERTICES_PER_MESH = 65000;
        public const int MAX_TRIANGLES_PER_MESH = ((MAX_VERTICES_PER_MESH / 3) * 3) / 3; // 64998 / 3 == 21666

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

        public static MemoryStream GetResourcePathByteStream(string resourcePath)
        {
            TextAsset textAsset = Resources.Load(resourcePath) as TextAsset;
            byte[] bytes = textAsset != null ? textAsset.bytes : null;
            return bytes != null ? new MemoryStream(bytes) : null;
        }

        public static GameObject LoadResource(string resourcePath)
        {
            Debug.Log((TAG + " LoadResource(resourcePath:" + Utils.Quote(resourcePath) + ")");
            MemoryStream stream = GetResourcePathByteStream(resourcePath);
            MeshInfo[] meshInfos = GetMeshInfos(stream);
            return GetGameObject(meshInfos);
        }

        public delegate void LoadCallback(GameObject gameObject);

        // TODO:(pv) Experiment w/ converting this to a Unity Coroutine
        public static void LoadResourceAsync(string resourcePath, LoadCallback callback)
        {
            if (VERBOSE_LOG)
            {
                Debug.Log((TAG + " LoadResourceAsync(resourcePath:" + Utils.Quote(resourcePath) +
                          ", callback:" + callback + ")");
            }
            MemoryStream stream = GetResourcePathByteStream(resourcePath);
            LoadAsync(stream, callback);
        }

        public static void LoadFileAsync(string filePath, LoadCallback callback)
        {
            if (VERBOSE_LOG)
            {
                Debug.Log((TAG + " LoadFileAsync(filePath:" + Utils.Quote(filePath) +
                          ", callback:" + callback + ")");
            }
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            LoadAsync(fileStream, callback);
        }

        private delegate MeshInfo[] GetMeshInfosCaller(Stream stream);

        public static void LoadAsync(Stream stream, LoadCallback callback)
        {
            if (VERBOSE_LOG)
            {
                Debug.Log((TAG + " LoadAsync(stream, callback");
            }
            IAsyncResult asyncResult = new GetMeshInfosCaller(GetMeshInfos)
                .BeginInvoke(stream, OnGetMeshInfos, callback);
            if (asyncResult.CompletedSynchronously)
            {
                OnGetMeshInfos(asyncResult);
            }
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

        private static MeshInfo[] GetMeshInfos(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            if (VERBOSE_LOG)
            {
                Debug.LogWarning((TAG + " GetMeshInfos: MAX_TRIANGLES_PER_MESH == " + MAX_TRIANGLES_PER_MESH);
            }

            //Debug.Log((TAG + " GetMeshInfos: new CtmFileReader(memoryStream)");
            CtmFileReader reader = new CtmFileReader(stream);
            //Debug.Log((TAG + " GetMeshInfos: reader.decode()");
            OpenCTM.Mesh ctmMesh = reader.decode();

            if (VERBOSE_LOG)
            {
                Debug.LogWarning((TAG + " GetMeshInfos: BEGIN Converting OpenCTM.Mesh to UnityEngine.Mesh...");
            }

            int verticesLength = ctmMesh.vertices.Length;
            //Debug.LogError((TAG + " GetMeshInfos: ctmMesh.vertices.Length == " + verticesLength);
            //int numVectors = verticesLength / 3;
            //Debug.LogError((TAG + " GetMeshInfos: numVectors == " + numVectors);
            List<Vector3> vertices = new List<Vector3>();
            for (int j = 0; j < verticesLength; j += 3)
            {
                vertices.Add(new Vector3(ctmMesh.vertices[j],
                                         ctmMesh.vertices[j + 1],
                                         ctmMesh.vertices[j + 2]));
            }
            //Debug.LogError((TAG + " GetMeshInfos: vertices.Count == " + vertices.Count);

            bool hasNormals = ctmMesh.normals != null;
            //Debug.LogError((TAG + " GetMeshInfos: hasNormals == " + hasNormals);
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
            //Debug.LogError((TAG + " GetMeshInfos: normals.Count == " + normals.Count);

            int indicesLength = ctmMesh.indices.Length;
            //Debug.LogError((TAG + " GetMeshInfos: ctmMesh.indices.Length == " + indicesLength);
            int numTriangles = indicesLength / 3;
            if (VERBOSE_LOG)
            {
                Debug.LogWarning((TAG + " GetMeshInfos: numTriangles == " + numTriangles);
            }

            //List<Vector2> uv = new List<Vector2>();
            /*
            // TODO:(pv) Texture...
            //Debug.LogError((TAG + " GetMeshInfos: ctmMesh.texcoordinates == " + ctmMesh.texcoordinates);
            if (ctmMesh.texcoordinates.Length > 0)
            {
                for (int j = 0; j < ctmMesh.texcoordinates[0].values.Length / 2; j++)
                {
                    uv.Add(new Vector2(ctmMesh.texcoordinates[0].values[(j * 2)],
                                       ctmMesh.texcoordinates[0].values[(j * 2) + 1]));
                }
            }
            */
            //Debug.LogError((TAG + " GetMeshInfos: uv.Count == " + uv.Count);

            int meshCount = (int)Math.Ceiling((double)(numTriangles / (double)MAX_TRIANGLES_PER_MESH));
            if (VERBOSE_LOG)
            {
                Debug.LogWarning((TAG + " GetMeshInfos: meshCount == " + meshCount);
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
                    Debug.LogError((TAG + " GetMeshInfos: triangleIndex:" + triangleIndex);
                }

                meshIndex = triangleIndex / MAX_TRIANGLES_PER_MESH;
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError((TAG + " GetMeshInfos: meshIndex:" + meshIndex);
                }

                meshInfo = meshInfos[meshIndex];
                if (meshInfo == null)
                {
                    if (triangleIndex >= debugTriangleIndex)
                    {
                        Debug.LogError((TAG + " GetMeshInfos: meshInfos[" + meshIndex + "] = new MeshInfo();");
                    }
                    meshInfo = new MeshInfo();
                    meshInfos[meshIndex] = meshInfo;
                }

                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError((TAG + " GetMeshInfos: meshInfo:" + meshInfo);
                }

                indicesIndex = triangleIndex * 3;
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError((TAG + " GetMeshInfos: indicesIndex:" + indicesIndex);
                }

                triangleVertex1 = ctmMesh.indices[indicesIndex];
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError((TAG + " GetMeshInfos: triangleVertex1:" + triangleVertex1);
                }
                meshInfo.triangles.Add(meshInfo.vertices.Count);
                vertex1 = vertices[triangleVertex1];
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError((TAG + " GetMeshInfos: vertex1:" + vertex1);
                }
                meshInfo.vertices.Add(vertex1);
                if (hasNormals)
                {
                    normal1 = normals[triangleVertex1];
                    if (triangleIndex >= debugTriangleIndex)
                    {
                        Debug.LogError((TAG + " GetMeshInfos: normal1:" + normal1);
                    }
                    meshInfo.normals.Add(normal1);
                }

                triangleVertex2 = ctmMesh.indices[indicesIndex + 1];
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError((TAG + " GetMeshInfos: triangleVertex2:" + triangleVertex2);
                }
                meshInfo.triangles.Add(meshInfo.vertices.Count);
                vertex2 = vertices[triangleVertex2];
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError((TAG + " GetMeshInfos: vertex2:" + vertex2);
                }
                meshInfo.vertices.Add(vertex2);
                if (hasNormals)
                {
                    normal2 = normals[triangleVertex2];
                    if (triangleIndex >= debugTriangleIndex)
                    {
                        Debug.LogError((TAG + " GetMeshInfos: normal2:" + normal2);
                    }
                    meshInfo.normals.Add(normal2);
                }

                triangleVertex3 = ctmMesh.indices[indicesIndex + 2];
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError((TAG + " GetMeshInfos: triangleVertex3:" + triangleVertex3);
                }
                meshInfo.triangles.Add(meshInfo.vertices.Count);
                vertex3 = vertices[triangleVertex3];
                if (triangleIndex >= debugTriangleIndex)
                {
                    Debug.LogError((TAG + " GetMeshInfos: vertex3:" + vertex3);
                }
                meshInfo.vertices.Add(vertex3);
                if (hasNormals)
                {
                    normal3 = normals[triangleVertex3];
                    if (triangleIndex >= debugTriangleIndex)
                    {
                        Debug.LogError((TAG + " GetMeshInfos: normal3:" + normal3);
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
                        Debug.LogError((TAG + " GetMeshInfos: triangleVertexMax:" + triangleVertexMax);
                    }
                }
            }

            return meshInfos;
        }

        private static GameObject GetGameObject(MeshInfo[] meshInfos)
        {
            // TODO:(pv) Set root [and children?] to be Occlusion Culling
            //  https://docs.unity3d.com/Manual/OcclusionCulling.html
            //  https://forum.unity.com/threads/dynamic-occlusion-culling-lod-for-unity-free.140704/
            //  https://www.assetstore.unity3d.com/en/#!/content/79736

            if (meshInfos == null)
            {
                return null;
            }

            GameObject root = new GameObject();
            //root.SetActive(false);

            if (VERBOSE_LOG)
            {
                Debug.LogWarning((TAG + " GetGameObject: Creating Unity Mesh(es)");
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
                    Debug.LogError((TAG + " GetGameObject: meshIndex:" + meshIndex);
                }

                meshInfo = meshInfos[meshIndex];
                if (meshIndex >= debugMeshIndex)
                {
                    Debug.LogError((TAG + " GetGameObject: meshInfo:" + meshInfo);
                    Debug.LogError((TAG + " GetGameObject: meshInfo.vertices.Count:" + meshInfo.vertices.Count);
                    Debug.LogError((TAG + " GetGameObject: meshInfo.normals.Count:" + meshInfo.normals.Count);
                    Debug.LogError((TAG + " GetGameObject: meshInfo.triangles.Count:" + meshInfo.triangles.Count);
                    Debug.LogError((TAG + " GetGameObject: meshInfo.uv.Count:" + meshInfo.uv.Count);
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

                if (true)
                {
                    MeshCollider mc = child.AddComponent<MeshCollider>();
                    mc.sharedMesh = unityMesh;
                }

                child.transform.SetParent(root.transform);
            }

            if (VERBOSE_LOG)
            {
                Debug.LogWarning((TAG + " GetGameObject: END Converting OpenCTM.Mesh to UnityEngine.Mesh");
            }

            return root;
        }
    }
}