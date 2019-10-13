#ifndef MCR_COMPUTESHADER_VARIABLES
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
#define MCR_COMPUTESHADER_VARIABLES
#include "Assets/GPUDRP/Shader/MeshClusterRendering/MCRStructer.cginc"

//用来做裁剪的cluster
StructuredBuffer<ClusterInfo> _MCRClusterBuffer;

//裁剪输出buffer
RWStructuredBuffer<uint> _MCRCullResultBuffer;

//视椎体的六个面
float4[6] _FrustumPlanes;

#endif