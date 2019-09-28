using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxWellGPUDrivenRenderPipeline
{
    /// <summary>
    /// Mesh Cluster Rendering System Manager
    /// 负责管理MCR System
    /// </summary>
    public static class MCRExecuterManager 
    {
        private static List<MCRExecuter> s_currentMCR = new List<MCRExecuter>();
        private static MCRClusterInfoTable s_mcrClusterTable = null;
        private static string s_mcrClusterTablePath = "MapMat/MCRClusterInfoTable";

        public static void AddExecuter(MCRExecuter sys)
        {
            if (!sys)
            {
                return;
            }

            if (!s_currentMCR.Contains(sys))
            {
                s_currentMCR.Add(sys);
            }
        }

        public static void RemoveExecuter(MCRExecuter sys)
        {
            if (!sys)
            {
                return;
            }

            s_currentMCR.Remove(sys);
        }

        public static void DrawAllExecuter(GPUDRPCamera gpudrpCamera)
        {
            foreach(var instance in s_currentMCR)
            {
                instance.DrawMCR(gpudrpCamera);
            }
        }

        public static MCRClusterInfo GetMCRClusterInfo(int index)
        {
            MCRClusterInfo outRes = default(MCRClusterInfo);
            if (!s_mcrClusterTable)
            {
                s_mcrClusterTable = Resources.Load<MCRClusterInfoTable>(s_mcrClusterTablePath);
                if(!s_mcrClusterTable)
                {
                    Debug.LogError("找不到MCRClusterInfoTable:" + s_mcrClusterTablePath);
                    return outRes;
                }
            }

            if(index < 0 || s_mcrClusterTable.clusterInfoList.Count <= index)
            {
                Debug.LogError("参数index不合法:" + index + ",[0-" + s_mcrClusterTable.clusterInfoList.Count + "]");
                return outRes;
            }

            outRes = s_mcrClusterTable.clusterInfoList[index];

            return outRes;
        }
    }

}
