#ifndef MCRVARIABLES
#define MCRVARIABLES

#include "Assets/GPUDRP/Scripts/MeshClusterRendering/MCRVertex.cginc"
#include "Assets/GPUDRP/Scripts/MeshClusterRendering/MCRCluster.cginc"

//一个Cluster有多少个三角形
#define CLUSTER_TRI_COUNT 64

//一个cluster有多少个顶点
#define CLUSTER_VERTEX_COUNT 192


#ifdef COMPUTESHADER
RWStructuredBuffer<MCRVertex> verticesBuffer;
RWStructuredBuffer<MCRCluster> clusterBuffer;
RWStructuredBuffer<uint> resultBuffer;
#else
StructuredBuffer<MCRVertex> verticesBuffer;
StructuredBuffer<uint> resultBuffer;
#endif



#endif