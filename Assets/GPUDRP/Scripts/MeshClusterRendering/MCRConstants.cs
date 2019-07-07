using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MCRConstant 
{
    public const int c_INDIRECTSIZE = 20;
    public const int c_CLUSTER_CLIP_COUNT = 255;
    public const int c_CLUSTER_VERTEX_COUNT = c_CLUSTER_CLIP_COUNT;
    public const int ClusterCull_Kernel = 0;
    public const int ClearCluster_Kernel = 1;
    public const int UnsafeCull_Kernel = 2;
    public const int MoveVertex_Kernel = 3;
    public const int MoveCluster_Kernel = 4;
    public const int FrustumFilter_Kernel = 5;
    public const int OcclusionRecheck_Kernel = 6;
    public const int ClearOcclusionData_Kernel = 7;
}
