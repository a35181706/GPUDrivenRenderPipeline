using Unity.Mathematics;

namespace GPUDrivenRenderPipeline
{
    /// <summary>
    /// Mesh Cluster Rendering顶点
    /// </summary>
    public struct MCRVertex
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

        /// <summary>
        /// 世界坐标
        /// </summary>
        public float3 position
        {
            get
            {
                return new float3(data1.x, data1.y, data1.z);
            }
            set
            {
                data1.x = value.x;
                data1.y = value.y;
                data1.z = value.z;
            }
        }

        /// <summary>
        /// 世界法线
        /// </summary>
        public float3 normal
        {
            get
            {
                return new float3(data2.x, data2.y, data2.z);
            }
            set
            {
                data2.x = value.x;
                data2.y = value.y;
                data2
.z = value.z;
            }
        }

        public float2 uv0
        {
            get
            {
                return new float2(data1.w, data2.w);
            }
            set
            {
                data1.w = value.x;
                data2.w = value.y
;
            }
        }

        public int textureIndex
        {
            set
            {
                data3.x = value;
            }
            get
            {
                return (int)(data3.x + 0.000001f);
            }
        }

    }
}