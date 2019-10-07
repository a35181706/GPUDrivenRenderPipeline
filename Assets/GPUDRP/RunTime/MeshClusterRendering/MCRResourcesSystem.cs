using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace GPUDRP.MeshClusterRendering
{
    public static class MCRResourcesSystem
    {
        private static bool bNeedToExit = false;
        private static bool bInit = false;

        private readonly static System.Object lockObj = new System.Object();

        private static Queue<MCRSceneContext> bakeAssetsQueue = new Queue<MCRSceneContext>();

        public static void Init()
        {
            if(bInit)
            {
                return;
            }
            bInit = true;
            bNeedToExit = false;
            System.Threading.ThreadPool.QueueUserWorkItem(BackGroundThread);
        }

        public static void Destroy()
        {
            if(!bInit)
            {
                return;
            }
            bInit = false;
            bNeedToExit = true;
        }

        //private static bool isHaveMission()
        //{
        //    return bakeAssetsQueue.Count > 0;
        //}

        private static void ProcessBakeAssetLoadingQueue()
        {
            MCRSceneContext mcrScenecontext = null;

            lock (lockObj)
            {
                if(bakeAssetsQueue.Count > 0)
                {
                    mcrScenecontext = bakeAssetsQueue.Dequeue();
                }
               
            }

            if (mcrScenecontext.bDestroyed)
            {
                return;
            }
            
            System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.OpenRead(mcrScenecontext.ClusterInfoAssetsPath));

            int clusterCount = reader.ReadInt32();
            int vertexCount = reader.ReadInt32();

            //读取cluster
            for (int i = 0;i < clusterCount; i++)
            {
                byte[] bytes = reader.ReadBytes(Marshal.SizeOf<ClusterInfo>());
                ClusterInfo clusterinfo = IOUtils.ByteToStruct<ClusterInfo>(bytes);

                if (mcrScenecontext.bDestroyed)
                {
                    reader.Close();
                    return;
                }
                mcrScenecontext.clusterList[i] = clusterinfo;
            }

            //读取顶点
            for (int i = 0; i < vertexCount; i++)
            {
                byte[] bytes = reader.ReadBytes(Marshal.SizeOf<VertexInfo>());
                VertexInfo vertexinfo = IOUtils.ByteToStruct<VertexInfo>(bytes);

                if (mcrScenecontext.bDestroyed)
                {
                    reader.Close();
                    return;
                }

                mcrScenecontext.vertexList[i] = vertexinfo;
            }

            if (mcrScenecontext.bDestroyed)
            {
                reader.Close();
                return;
            }

            mcrScenecontext.bLoadFinish = true;

            reader.Close();
        }

        private static void BackGroundThread(System.Object data)
        {
            while(true)
            {
                if (bNeedToExit)
                {
                    return;
                }

                //if(!isHaveMission())
                //{
                    System.Threading.Thread.Sleep(30);
                    //continue;
                //}

                ProcessBakeAssetLoadingQueue();
            }
        }


        #region API
        
        public static void LoadMCRBakeAsset(MCRSceneContext scene)
        {
            if(null == scene)
            {
                return;
            }


            lock(lockObj)
            {
                bakeAssetsQueue.Enqueue(scene);
            }
        }

        public static void UnLoadMCRBakeAssets(MCRSceneContext scene)
        {
            if (null == scene)
            {
                return;
            }

        }

        #endregion
    }

}
