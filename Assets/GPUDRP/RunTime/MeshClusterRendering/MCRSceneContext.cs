using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
namespace GPUDRP.MeshClusterRendering
{
    [System.Serializable]
    public class MCRSceneContext
    {
        /// <summary>
        /// 来源于序列化数据
        /// </summary>
        public string ClusterInfoAssetsPath = string.Empty;

        /// <summary>
        /// 来源于序列化数据,baker负责填充
        /// </summary>
        public int ClusterCount = 0;
        /// <summary>
        /// 来源于序列化数据,baker负责填充
        /// </summary>
        public int VertexCount = 0;

        [System.NonSerialized]
        public NativeArray<ClusterInfo> clusterList;
        public int clusterStride { get; private set; }
        public ComputeBuffer clusterBuffer { get; set; }

        [System.NonSerialized]
        public NativeArray<VertexInfo> vertexList;
        public int vertexStride { get; private set; }
        public ComputeBuffer vertexBuffer { get; set; }

        /// <summary>
        /// 有加载线程设置为true
        /// </summary>
        public bool bLoadFinish{get; set;}

        public bool bDestroyed { get; private set; }

        public List<uint> indirectArgs { get; set; }
        public ComputeBuffer inDirectBuffer { get; set; }


        public uint []cullResultList { get; set; }
        public ComputeBuffer cullResultBuffer { get; set; }


        public void Init(MCRScene scene)
        {
            clusterStride = Marshal.SizeOf<ClusterInfo>();
            vertexStride = Marshal.SizeOf<VertexInfo>();

            clusterList = new Unity.Collections.NativeArray<ClusterInfo>(ClusterCount, Unity.Collections.Allocator.Persistent);
            vertexList = new Unity.Collections.NativeArray<VertexInfo>(VertexCount, Unity.Collections.Allocator.Persistent);
            cullResultList = new uint[ClusterCount + 1];

            indirectArgs = new List<uint> { 0, 0, 0, 0, 0 };
            vertexBuffer = ComputeBufferPool.GetTempBuffer(VertexCount, vertexStride);
            clusterBuffer = ComputeBufferPool.GetTempBuffer(ClusterCount, clusterStride);
            cullResultBuffer = ComputeBufferPool.GetTempBuffer(ClusterCount + 1, 4);

            inDirectBuffer = ComputeBufferPool.GetIndirectBuffer();

            ClusterInfoAssetsPath = MCRConstant.BakeAssetSavePath + "/" + ClusterInfoAssetsPath;
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                //ClusterInfoAssetsPath = MCRConstant.BakeAssetSavePath + "/" + ClusterInfoAssetsPath;
            }
            else
            {
                scene.StartCoroutine(LoadingBytes());
            }


            bLoadFinish = false;
            bDestroyed = false;
        }

        IEnumerator LoadingBytes()
        {
            while(true)
            {
                UnityWebRequest req = UnityWebRequest.Get(ClusterInfoAssetsPath);
                yield return req.SendWebRequest();

                byte[] allData = req.downloadHandler.data;
                byte[] intBytes = new byte[4];
                
                int position = 0;
                System.Array.Copy(allData, position,intBytes, 0,4);
                int clusterCount = IOUtils.ByteToStruct<int>(intBytes);
                position += 4;

                System.Array.Copy(allData, position, intBytes, 0, 4);
                int vertexCount = IOUtils.ByteToStruct<int>(intBytes);
                position += 4;
                intBytes = null;

                byte[] clusterByte = new byte[clusterStride];
                byte[] vertexByte = new byte[vertexStride];

                for (int i = 0; i < clusterCount; i++)
                {
                    System.Array.Copy(allData, position, clusterByte, 0, clusterStride);
                    ClusterInfo clusterinfo = IOUtils.ByteToStruct<ClusterInfo>(clusterByte);

                    clusterList[i] = clusterinfo;
                    position += clusterStride;
                }

                for (int i = 0; i < vertexCount; i++)
                {
                    System.Array.Copy(allData, position, vertexByte, 0, vertexStride);
                    VertexInfo vertexinfo = IOUtils.ByteToStruct<VertexInfo>(vertexByte);

                    vertexList[i] = vertexinfo;
                    position += vertexStride;
                }

                clusterByte = null;
                vertexByte = null;
                
                req.Dispose();
                bLoadFinish = true;
                yield break;
            }
        }


        public void Destroy()
        {

            bDestroyed = true;
            ComputeBuffer temp = vertexBuffer;
            ComputeBufferPool.ReleaseTempBuffer(ref temp);
            temp = clusterBuffer;
            ComputeBufferPool.ReleaseTempBuffer(ref temp);
            temp = inDirectBuffer;
            ComputeBufferPool.ReleaseTempBuffer(ref temp);
            temp = cullResultBuffer;
            ComputeBufferPool.ReleaseTempBuffer(ref temp);

            clusterList.Dispose();
            vertexList.Dispose();

            cullResultList = null;
            indirectArgs = null;
            bLoadFinish = false;
         
        }

    }

}
