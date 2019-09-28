using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.IO;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;
using UnityEditor;
namespace MaxWellGPUDrivenRenderPipeline
{
    [Serializable]
    public struct Pair<T, V>
    {
        public T key;
        public V value;
        public Pair(T key, V value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public struct Pair
    {
        public string key;
        public Texture2DArray value;
        public Pair(string key, Texture2DArray value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public struct CombinedModel
    {
        public NativeList<MCRVertex> allVertex;
        public NativeList<int> triangles;
        public Bounds bound;
    }

    public unsafe class SceneClusterGenerator : EditorWindow
    {
        public int voxelCount = 100;

        public GameObject SceneRoot;


        [MenuItem("MaxWellGPUDRP/Tools/SceneClusterGenerator")]
        public static void Open()
        {
            EditorWindow.GetWindow<SceneClusterGenerator>("场景Cluster生成器");
        }

        private void OnGUI()
        {
            voxelCount = EditorGUILayout.IntSlider("voxelCount:",voxelCount, 100, 500);
            SceneRoot = EditorGUILayout.ObjectField("根节点:", SceneRoot, typeof(GameObject), true) as GameObject;

            if(SceneRoot)
            {
                if (GUILayout.Button("生成ClusterInfo"))
                {
                    GenerateSceneRoot(SceneRoot);
                    SceneRoot = null;
                }
                
            }

        }

        public void GetVertexs(NativeList<MCRVertex> vertexs, NativeList<int> triangles, Mesh targetMesh, Transform transform)
        {
            int originLength = vertexs.Length;
            Vector3[] vertices = targetMesh.vertices;
            vertexs.AddRange(vertices.Length);
            for (int i = originLength; i < vertices.Length + originLength; ++i)
            {
                int len = i - originLength;

                vertexs[i].position = transform.localToWorldMatrix.MultiplyPoint(vertices[len]); ;
                vertexs[i].uv0 = targetMesh.uv[len];
                vertexs[i].normal = transform.localToWorldMatrix.MultiplyVector(targetMesh.normals[len]);

            }
            for (int subCount = 0; subCount < targetMesh.subMeshCount; ++subCount)
            {
                int[] triangleArray = targetMesh.GetTriangles(subCount);

                for (int i = 0; i < triangleArray.Length; ++i)
                {
                    triangleArray[i] += originLength;
                }
                triangles.AddRange(triangleArray);
            }

        }

        public CombinedModel ProcessCluster(MeshRenderer[] allRenderers, Dictionary<MeshRenderer, bool> lowLODLevels )
        {
            List<MeshFilter> allFilters = new List<MeshFilter>(allRenderers.Length);
            int sumVertexLength = 0;
            int sumTriangleLength = 0;

            for (int i = 0; i < allRenderers.Length; ++i)
            {
                if (!lowLODLevels.ContainsKey(allRenderers[i]))
                {
                    MeshFilter filter = allRenderers[i].GetComponent<MeshFilter>();
                    allFilters.Add(filter);
                    sumVertexLength += filter.sharedMesh.vertexCount;
                }
            }
            sumTriangleLength = (int)(sumVertexLength * 1.5);
            NativeList<MCRVertex> vertex = new NativeList<MCRVertex>(sumVertexLength, Allocator.Temp);
            NativeList<int> triangles = new NativeList<int>(sumTriangleLength, Allocator.Temp);
            for (int i = 0; i < allFilters.Count; ++i)
            {
                Mesh mesh = allFilters[i].sharedMesh;
                GetVertexs(vertex, triangles, mesh, allFilters[i].transform);
            }
            float3 less = vertex[0].position;
            float3 more = vertex[0].position;

            for (int i = 1; i < vertex.Length; ++i)
            {
                float3 current = vertex[i].position;
                if (less.x > current.x) less.x = current.x;
                if (more.x < current.x) more.x = current.x;
                if (less.y > current.y) less.y = current.y;
                if (more.y < current.y) more.y = current.y;
                if (less.z > current.z) less.z = current.z;
                if (more.z < current.z) more.z = current.z;
            }

            float3 center = (less + more) / 2;
            float3 extent = more - center;
            Bounds b = new Bounds(center, extent * 2);
            CombinedModel md;
            md.bound = b;
            md.allVertex = vertex;
            md.triangles = triangles;

            return md;
        }

        private string GetCurrentSceneName()
        {
            if (!SceneRoot)
            {
                return string.Empty;
            }

            return SceneRoot.scene.name;
        }

        public void GenerateSceneRoot(GameObject root)
        {
            MCRExecuter system = root.GetComponent<MCRExecuter>();
            if (!system)
            {
                system = root.AddComponent<MCRExecuter>();
            }
            bool save = false;
            MCRClusterInfoTable res = Resources.Load<MCRClusterInfoTable>("MapMat/MCRClusterInfoTable");
            if (res == null)
            {
                save = true;
                res = ScriptableObject.CreateInstance<MCRClusterInfoTable>();
                res.name = "MCRClusterInfoTable";
                res.clusterInfoList = new List<MCRClusterInfo>();
            }
            Func<MCRClusterInfo, MCRClusterInfo, bool> equalCompare = (a, b) =>
            {
                return a.name == b.name;
            };
            MCRClusterInfo property = new MCRClusterInfo();
            property.name = GetCurrentSceneName();

            int index = -1;
            for(int i = 0;i < res.clusterInfoList.Count;i++)
            {
                var instanc = res.clusterInfoList[i];
                if (equalCompare(property, instanc))
                {
                    index = i;
                    break;
                }
            }
            LODGroup[] groups = root.GetComponentsInChildren<LODGroup>();
            Dictionary<MeshRenderer, bool> lowLevelDict = new Dictionary<MeshRenderer, bool>();
            foreach(var i in groups)
            {
                LOD[] lods = i.GetLODs();
                for(int j = 1; j < lods.Length; ++j)
                {
                    foreach(var k in lods[j].renderers)
                    {
                        if(k.GetType() == typeof(MeshRenderer))
                            lowLevelDict.Add(k as MeshRenderer, true);
                    }
                }
            }
           
            CombinedModel model = ProcessCluster(root.GetComponentsInChildren<MeshRenderer>(), lowLevelDict);

            property.clusterCount = MCRGenerator.GenerateCluster(model.allVertex, model.triangles, model.bound, GetCurrentSceneName(), voxelCount, index < 0 ? res.clusterInfoList.Count : index);

            if(index < 0)
            {
                res.clusterInfoList.Add(property);
            }
            else
            {
                res.clusterInfoList[index] = property;
            }
            system.clusterInfoTableIndex = res.clusterInfoList.Count - 1;
            if (save)
                AssetDatabase.CreateAsset(res, "Assets/Resources/MapMat/MCRClusterInfoTable.asset");
            else
                EditorUtility.SetDirty(res);
        }

    }

}
