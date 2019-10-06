using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDRP
{

    public static class ComputeBufferPool
    {
        private static List<ComputeBuffer> waitForRelease = new List<ComputeBuffer>(10);

        public static ComputeBuffer GetTempBuffer(int length,int stride)
        {
            return new ComputeBuffer(length, stride);
        }

        /// <summary>
        /// indirectbuffer参数，分别代表
        /// 0-indexbuffer的大小
        /// 1-绘制多少个实例
        /// 2-indexbuffer的开始位置
        /// 3-vertexbuffer的开始位置
        /// 4-未知
        /// </summary>
        /// <returns></returns>
        public static ComputeBuffer GetIndirectBuffer()
        {
            return new ComputeBuffer(1, 20, ComputeBufferType.IndirectArguments);
        }

        public static void ReleaseTempBuffer(ref ComputeBuffer buffer)
        {
            if(null == buffer)
            {
                return;
            }

            buffer.Release();
            buffer = null;
        }

        public static void ReleaseEndOfRender(ref ComputeBuffer buffer)
        {
            if (null == buffer)
            {
                return;
            }

            waitForRelease.Add(buffer);
            buffer = null;
        }

        public static void EndOfRender()
        {
            foreach(ComputeBuffer buffer in waitForRelease)
            {
                buffer.Release();
            }

            waitForRelease.Clear();
        }

        public static void Destroy()
        {

        }
    }

}
