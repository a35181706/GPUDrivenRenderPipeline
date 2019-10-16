#ifndef MCR_STRUCTER
#define MCR_STRUCTER

struct ClusterInfo
{
	/// <summary>
	/// XYZ:WorldPos
	/// W:unused
	/// </summary>
	float4 center;

	float3 extent;
	int clusterIndex;
};

struct VertexInfo
{
	/// <summary>
	/// XYZ:WorldPos
	/// W:uv0.x
	/// </summary>
	float4 data1;

	/// <summary>
	/// XYZ:WorldNormal
	/// W:uv0.y
	/// </summary>
	float4 data2;

	/// <summary>
	/// XYZW:Color
	/// </summary>
	float4 data3;
};
#endif