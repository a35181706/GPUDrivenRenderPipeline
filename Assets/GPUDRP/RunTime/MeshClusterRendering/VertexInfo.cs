using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUDRP.MeshClusterRendering
{
    public class VertexInfo
    {
        /// <summary>
        /// XYZ:WorldPos
        /// W:uv0.x
        /// </summary>
        private Vector4 data1;

        /// <summary>
        /// XYZ:WorldNormal
        /// W:uv0.y
        /// </summary>
        private Vector4 data2;

        /// <summary>
        /// XYZW:Color
        /// </summary>
        private Vector4 data3;


        public Vector3 worldPos
        {
            get
            {
                return data1;
            }
            set
            {
                data1 = value;
            }
        }

        public Vector3 worldNormal
        {
            get
            {
                return data2;
            }
            set
            {
                data2 = value;
            }
        }

        public Vector4 Color
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

        public Vector2 UV0
        {
            get
            {
                return new Vector2(data1.w, data2.w);
            }
            set
            {
                data1.w = value.x;
                data2.w = value.y;
            }
        }
    }

}
