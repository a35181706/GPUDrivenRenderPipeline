using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MCRConstant 
{
    /// <summary>
    /// 一个Cluster有多少个三角形,这个后面需要根据GPU的wave数量来定义，提高效率
    /// </summary>
    public const int c_CLUSTER_TRI_COUNT = 64;

    /// <summary>
    /// 一个cluster有多少个顶点
    /// </summary>
    public const int c_CLUSTER_VERTEX_COUNT = c_CLUSTER_TRI_COUNT * 3;

    public const int ClusterCull_Kernel = 0;
    public const int ClearCluster_Kernel = 1;
    public const int UnsafeCull_Kernel = 2;
    public const int MoveVertex_Kernel = 3;
    public const int MoveCluster_Kernel = 4;
    public const int FrustumFilter_Kernel = 5;
    public const int OcclusionRecheck_Kernel = 6;
    public const int ClearOcclusionData_Kernel = 7;
}
