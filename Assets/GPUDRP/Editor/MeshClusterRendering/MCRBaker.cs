using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.IO;
using Unity.Mathematics;

namespace GPUDRP.MeshClusterRendering
{
    public class MCRBaker :EditorWindow
    {
        /// <summary>
        /// 在这个根节点下的，才会生成MCR
        /// </summary>
        const string MCRParentRoot = "MCRRoot";

        [MenuItem("GPUDRP/Tools/MCRBaker")]
        public static void OpenEditor()
        {
            EditorWindow.GetWindow<MCRBaker>(false, "MCRBaker");
        }


        private void OnGUI()
        {
            if(GUILayout.Button("Bake"))
            {
                Bake();
            }
        }

        void Bake()
        {
            GameObject parentRoot = GameObject.Find(MCRParentRoot);


            if (!parentRoot)
            {
                return;
            }

            MeshFilter[] allFilters = parentRoot.GetComponentsInChildren<MeshFilter>();

            if(allFilters.Length < 0)
            {
                return;
            }

            MCRScene mcrScene = parentRoot.GetComponent<MCRScene>();
            if(!mcrScene)
            {
                mcrScene = parentRoot.AddComponent<MCRScene>();
            }

            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string fullSavedAssetsPath = MCRConstant.BakeAssetSavePath + "/" + currentSceneName + "_MCRBakeAsset" + ".mcr";

            List<ClusterInfo> allClusterList = new List<ClusterInfo>();
            List<VertexInfo> allVertexList = new List<VertexInfo>();

            //提取cluster
            foreach(MeshFilter mf in allFilters)
            {
                if(!mf.sharedMesh || !mf.gameObject.activeInHierarchy)
                {
                    continue;
                }

                MeshRenderer render = mf.GetComponent<MeshRenderer>();
                if (!render)
                {
                    continue;
                }

                if(render.sharedMaterials.Length != mf.sharedMesh.subMeshCount)
                {
                    Debug.LogError(mf.gameObject.name + "材质数量与submesh不匹配" +  ",submeshcout:" + mf.sharedMesh.subMeshCount + ",材质数量:" + render.sharedMaterials.Length);
                    continue;
                }

                //if(mf.sharedMesh.subMeshCount > 1)
                //{
                //    Debug.LogError("不支持多submesh的物体：" + mf.gameObject.name + ",submeshcout:" + mf.sharedMesh.subMeshCount);
                //    continue;
                //}

                List<Vector3> vertex = new List<Vector3>();
                List<int> triangles = new List<int>();
                List<Vector2> uv = new List<Vector2>();
                List<Vector3> normal = new List<Vector3>();
                List<Color> color = new List<Color>();

                mf.sharedMesh.GetVertices(vertex);
                mf.sharedMesh.GetUVs(0, uv);
                mf.sharedMesh.GetColors(color);
                mf.sharedMesh.GetNormals(normal);

                //处理submesh
                for(int k = 0;k < mf.sharedMesh.subMeshCount;k++)
                {
                    triangles.Clear();
                    mf.sharedMesh.GetTriangles(triangles, k);

                    //读取三角形顶点信息
                    for (int i = 0; i < triangles.Count; i += 3)
                    {
                        VertexInfo vertex1 = new VertexInfo();
                        int index0 = triangles[i];
                        vertex1.worldPos = mf.transform.localToWorldMatrix.MultiplyPoint(vertex[index0]);
                        vertex1.worldNormal = mf.transform.localToWorldMatrix.MultiplyVector(normal[index0]);
                        vertex1.UV0 = uv[index0];
                        //看看有没有顶点色
                        if (color.Count > 0)
                        {
                            vertex1.Color = new float4(color[index0].r, color[index0].g, color[index0].b, color[index0].a);
                        }

                        allVertexList.Add(vertex1);

                        VertexInfo vertex2 = new VertexInfo();
                        int index1 = triangles[i + 1];
                        vertex2.worldPos = mf.transform.localToWorldMatrix.MultiplyPoint(vertex[index1]);
                        vertex2.worldNormal = mf.transform.localToWorldMatrix.MultiplyVector(normal[index1]);
                        vertex2.UV0 = uv[index1];
                        //看看有没有顶点色
                        if (color.Count > 0)
                        {
                            vertex2.Color = new float4(color[index1].r, color[index1].g, color[index1].b, color[index1].a);
                        }

                        allVertexList.Add(vertex2);

                        VertexInfo vertex3 = new VertexInfo();
                        int index2 = triangles[i + 2];
                        vertex3.worldPos = mf.transform.localToWorldMatrix.MultiplyPoint(vertex[index2]);
                        vertex3.worldNormal = mf.transform.localToWorldMatrix.MultiplyVector(normal[index2]);
                        vertex3.UV0 = uv[index2];

                        //看看有没有顶点色
                        if (color.Count > 0)
                        {
                            vertex3.Color = new float4(color[index2].r, color[index2].g, color[index2].b, color[index2].a);
                        }

                        allVertexList.Add(vertex3);

                        //完成了一个Cluster所需要的顶点数
                        if ((allVertexList.Count % MCRConstant.CLUSTER_VERTEX_COUNT) == 0 && allVertexList.Count > 0)
                        {
                            allClusterList.Add(BuildCluster(allVertexList));
                        }
                    }
                }

            }

            int packCout = allVertexList.Count - (MCRConstant.CLUSTER_VERTEX_COUNT * allClusterList.Count);
            //完了之后看看顶点数是否刚好够，不够的话，补上去
            if (packCout > 0)
            {
                int dt = MCRConstant.CLUSTER_VERTEX_COUNT - packCout;
                for(int i = 0;i < dt;i++)
                {
                    VertexInfo info = allVertexList[allVertexList.Count - 1];
                    allVertexList.Add(info);
                }

                allClusterList.Add(BuildCluster(allVertexList));
            }

            //保存cluster
            SaveClustetInfo(allClusterList, allVertexList,fullSavedAssetsPath);
            mcrScene.context.ClusterInfoAssetsPath = fullSavedAssetsPath.Replace(MCRConstant.BakeAssetSavePath + "/",string.Empty);
            mcrScene.context.ClusterCount = allClusterList.Count;
            mcrScene.context.VertexCount = allVertexList.Count; 
        }


        public ClusterInfo BuildCluster(List<VertexInfo> allVertexList)
        {
            ClusterInfo clusterInfo = new ClusterInfo();
            clusterInfo.vertexStartIndex = allVertexList.Count - MCRConstant.CLUSTER_VERTEX_COUNT;

            //计算bounds,找出最大的点和最小的点
            float4 minPoint = float.MaxValue;
            minPoint.w = 0;
            float4 maxPoint = float.MinValue;
            maxPoint.w = 0;
            for (int index = clusterInfo.vertexStartIndex; index < allVertexList.Count; index++)
            {
                VertexInfo vertexInfo = allVertexList[index];

                //x
                if (vertexInfo.worldPos.x < minPoint.x)
                {
                    minPoint.x = vertexInfo.worldPos.x;
                }
                else if (vertexInfo.worldPos.x > maxPoint.x)
                {
                    maxPoint.x = vertexInfo.worldPos.x;
                }

                //y
                if (vertexInfo.worldPos.y < minPoint.y)
                {
                    minPoint.y = vertexInfo.worldPos.y;
                }
                else if (vertexInfo.worldPos.y > maxPoint.y)
                {
                    maxPoint.y = vertexInfo.worldPos.y;
                }

                //z
                if (vertexInfo.worldPos.z < minPoint.z)
                {
                    minPoint.z = vertexInfo.worldPos.z;
                }
                else if (vertexInfo.worldPos.z > maxPoint.z)
                {
                    maxPoint.z = vertexInfo.worldPos.z;
                }
            }

            clusterInfo.center = (minPoint + maxPoint) / 2;
            clusterInfo.extent = (maxPoint - minPoint) / 2;

            return clusterInfo;
        }

        public void SaveClustetInfo(List<ClusterInfo> clusterlist,List<VertexInfo> vertexList,string fullSavedAssetsPath)
        {
            
            IOUtils.DeleteFile(fullSavedAssetsPath);
            List<byte> allBytes = new List<byte>();

            //cluster数量
            allBytes.AddRange(IOUtils.StructToByte<int>(clusterlist.Count));

            //顶点数量
            allBytes.AddRange(IOUtils.StructToByte<int>(vertexList.Count));

            //写入cluster
            foreach (ClusterInfo cluster in clusterlist)
            {
                allBytes.AddRange(IOUtils.StructToByte<ClusterInfo>(cluster));
            }

            //写入vertex
            foreach (VertexInfo vertex in vertexList)
            {
                allBytes.AddRange(IOUtils.StructToByte<VertexInfo>(vertex));
            }

            IOUtils.WriteAllBytes(fullSavedAssetsPath, allBytes.ToArray());
        }
    }

}
