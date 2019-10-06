using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;

namespace GPUDRP.MeshClusterRendering
{
    public unsafe struct VertexInfo
    {
        /// <summary>
        /// XYZ:WorldPos
        /// W:uv0.x
        /// </summary>
        private float4 data1;

        /// <summary>
        /// XYZ:WorldNormal
        /// W:uv0.y
        /// </summary>
        private float4 data2;

        /// <summary>
        /// XYZW:Color
        /// </summary>
        private float4 data3;


        public float3 worldPos
        {
            get
            {
                return new float3(data1.x,data1.y,data1.z);
            }
            set
            {
                data1.x = value.x;
                data1.y = value.y;
                data1.z = value.z;
            }
        }

        public float3 worldNormal
        {
            get
            {
                return new float3(data2.x, data2.y, data2.z);
            }
            set
            {
                data2.x = value.x;
                data2.y = value.y;
                data2.z = value.z;
            }
        }

        public float4 Color
        {
            get
            {
                return data3;

            }
            set
            {
                data3 = value;
            }
        }

        public float2 UV0
        {
            get
            {
                return new float2(data1.w, data2.w);
            }
            set
            {
                data1.w = value.x;
                data2.w = value.y;
            }
        }
    }

}
