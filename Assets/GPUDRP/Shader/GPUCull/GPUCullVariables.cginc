#ifndef GPU_CULL_VARIABLES
#define GPU_CULL_VARIABLES
#include "Assets/GPUDRP/Shader/MeshClusterRendering/MCRStructer.cginc"

//用来做裁剪的cluster
StructuredBuffer<ClusterInfo> _MCRClusterBuffer;

//裁剪输出buffer
RWStructuredBuffer<uint> _MCRCullResultBuffer;
//裁剪输出的裁剪buffer
RWStructuredBuffer<uint> _MCRCullInstanceCountBuffer;
//视椎体的六个面
shared float4 _FrustumPlanes[6];

#endif