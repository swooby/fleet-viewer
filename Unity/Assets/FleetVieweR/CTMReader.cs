#if UNITY_ANDROID && !UNITY_EDITOR
#define RUNNING_ON_ANDROID_DEVICE
#endif  // UNITY_ANDROID && !UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using OpenCTM;

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

    // TODO:(pv) Make this [or upstream caller] an async task so that we don't block...
    //  https://www.google.com/search?q=unity+async+load
    public static GameObject Read(string name, string resourcePath)
    {
        Debug.Log("CTMReader.Read(resourcePath:" + Utils.Quote(resourcePath) + ")");
        if (VERBOSE_LOG)
        {
            Debug.LogWarning("CTMReader.Read: MAX_TRIANGLES_PER_MESH == " + MAX_TRIANGLES_PER_MESH);
        }

        GameObject root = new GameObject();
        root.name = name;

        TextAsset textAsset = Resources.Load(resourcePath) as TextAsset;
        Stream memoryStream = new MemoryStream(textAsset.bytes);

        //Debug.Log("CTMReader.Read: new CtmFileReader(memoryStream)");
        CtmFileReader reader = new CtmFileReader(memoryStream);
        //Debug.Log("CTMReader.Read: reader.decode()");
        OpenCTM.Mesh ctmMesh = reader.decode();
        //Debug.Log("CTMReader.Read: ctmMesh.checkIntegrity()");
        ctmMesh.checkIntegrity();

        if (VERBOSE_LOG)
        {
            Debug.LogWarning("CTMReader.Read: BEGIN Converting OpenCTM.Mesh to UnityEngine.Mesh...");
        }

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
        if (VERBOSE_LOG)
        {
            Debug.LogWarning("CTMReader.Read: numTriangles == " + numTriangles); // nox: ?, brunnen: ?
        }

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

        int meshCount = numTriangles / MAX_TRIANGLES_PER_MESH + 1;
        if (VERBOSE_LOG)
        {
            Debug.LogWarning("CTMReader.Read: meshCount == " + meshCount);
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
        for (int triangleIndex = 0; triangleIndex < numTriangles; triangleIndex++)
        {
            //Debug.LogError("CTMReader.Read: triangleIndex == " + triangleIndex);

            meshIndex = triangleIndex / MAX_TRIANGLES_PER_MESH;
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

            // TODO:(pv) Set root [and children?] to be Occlusion Culling
            //  https://docs.unity3d.com/Manual/OcclusionCulling.html

        if (VERBOSE_LOG)
        {
            Debug.LogWarning("CTMReader.Read: Creating Unity Mesh(es)");
        }
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
            Debug.LogWarning("CTMReader.Read: END Converting OpenCTM.Mesh to UnityEngine.Mesh");
        }

        return root;
    }
}