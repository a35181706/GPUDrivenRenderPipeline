#ifndef GPU_CULL_CONSTANT
#define GPU_CULL_CONSTANT
#include "Assets/GPUDRP/Shader/MeshClusterRendering/MCRConstant.cginc"

//视椎体裁剪的线程数量
#define FRUSTUM_NUM_THREADS CLUSTER_TRANGLES_COUNT
#endif