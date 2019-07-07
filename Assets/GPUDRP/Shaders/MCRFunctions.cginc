#ifndef MCRFUNCTIONS
#define MCRFUNCTIONS


MCRVertex getVertex(uint vertexID, uint instanceID)
{

	instanceID = resultBuffer[instanceID];
	uint vertexOffset = instanceID * CLUSTER_CLIP_COUNT;
	return verticesBuffer[vertexOffset + vertexID];
}


float3 DecodeWorldPos(MCRVertex vertex)
{
	return vertex.data1.xyz;

}
void EncodeWorldPos(MCRVertex vertex, float3 pos)
{
	vertex.data1.xyz = pos.xyz;
}

float3 DecodeWorldNormal(MCRVertex vertex)
{
	return vertex.data2.xyz;

}

float2 DecodeUV0(MCRVertex vertex)
{
	return float2(vertex.data1.w, vertex.data2.w);

}

float DecodeTextureIndex0_Float(MCRVertex vertex)
{
	return vertex.data3.x;

}


#endif