#ifndef MCRVERTEX
#define MCRVERTEX

/// <summary>
/// Mesh Cluster Rendering顶点
/// </summary>
struct MCRVertex
{
	/// <summary>
	/// xyz:世界坐标
	/// w:uv.x
	/// </summary>
	float4 data1;

	/// <summary>
	/// xyz:世界坐标下的法线
	/// w:uv.y
	/// </summary>
	float4 data2;

	/// <summary>
	/// x:贴图索引
	/// yzw:尚未使用
	/// </summary>
	float4 data3;
};

#endif
