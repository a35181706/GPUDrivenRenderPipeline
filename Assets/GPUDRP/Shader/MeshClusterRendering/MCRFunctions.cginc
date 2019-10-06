#ifndef MCR_FUNCTIONS
#define MCR_FUNCTIONS
#include "Assets/GPUDRP/Shader/MeshClusterRendering/MCRStructer.cginc"
#include "Assets/GPUDRP/Shader/MeshClusterRendering/MCRConstant.cginc"
#include "Assets/GPUDRP/Shader/MeshClusterRendering/MCRVariables.cginc"
VertexInfo getVertex(uint vertexID, uint instanceID)
{

	//instanceID = resultBuffer[instanceID];
	uint vertexOffset = instanceID * CLUSTER_VERTEX_COUNT;
	return _MCRVertexBuffer[vertexOffset + vertexID];
}


float4 DecodeWorldPos(VertexInfo vertex)
{
	return float4(vertex.data1.xyz,1);

}

float3 DecodeWorldNormal(VertexInfo vertex)
{
	return vertex.data2.xyz;

}

float2 DecodeUV0(VertexInfo vertex)
{
	return float2(vertex.data1.w, vertex.data2.w);

}

float4 DecodeVertexColor(VertexInfo vertex)
{
	return vertex.data3;

}

#endif