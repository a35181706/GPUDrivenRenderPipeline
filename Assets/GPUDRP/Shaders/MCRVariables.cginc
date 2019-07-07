#ifndef MCRVARIABLES
#define MCRVARIABLES

#include "Assets/GPUDRP/Scripts/MeshClusterRendering/MCRVertex.cginc"
#include "Assets/GPUDRP/Scripts/MeshClusterRendering/MCRCluster.cginc"

#define CLUSTER_CLIP_COUNT 255
#define CLUSTER_VERTEX_COUNT 255
#define PLANECOUNT 6



#ifdef COMPUTESHADER
RWStructuredBuffer<MCRVertex> verticesBuffer;
RWStructuredBuffer<MCRCluster> clusterBuffer;
RWStructuredBuffer<uint> resultBuffer;
#else
StructuredBuffer<MCRVertex> verticesBuffer;
StructuredBuffer<uint> resultBuffer;
#endif



#endif