#ifndef MCR_VARIABLES
#define MCR_VARIABLES
#include "Assets/GPUDRP/Shader/MeshClusterRendering/MCRStructer.cginc"

//顶点buffer
StructuredBuffer<VertexInfo> _MCRVertexBuffer;
//裁剪结果
StructuredBuffer<uint> _MCRCullResultBuffer;

#endif