#ifndef GPU_CULL_VARIABLES
#define GPU_CULL_VARIABLES
#include "Assets/GPUDRP/Shader/MeshClusterRendering/MCRStructer.cginc"

//用来做裁剪的cluster
StructuredBuffer<ClusterInfo> _MCRClusterBuffer;

//裁剪输出buffer
RWStructuredBuffer<uint> _MCRCullResultBuffer;

//视椎体的六个面
float4 _FrustumPlanes[6];

//GPU裁剪信息，x-cluster数目，yzw-尚未启用
float4 _GPUCullInfo;
#endif