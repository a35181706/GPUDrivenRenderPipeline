using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComputeBufferPool
{
    private static Dictionary<int, ComputeBuffer> s_allTempBuffers = new Dictionary<int, ComputeBuffer>(11);

    public static ComputeBuffer GetTempBuffer(int length, int stride)
    {
        ComputeBuffer target;
        if (s_allTempBuffers.TryGetValue(stride, out target))
        {
            if (target.count < length)
            {
                target.Dispose();
                target = new ComputeBuffer(length, stride);
                s_allTempBuffers[stride] = target;
            }
            return target;
        }
        else
        {
            target = new ComputeBuffer(length, stride);
            s_allTempBuffers[stride] = target;
            return target;
        }
    }

    public static void Release()
    {
        foreach(var buffer in s_allTempBuffers.Values)
        {
            if(null != buffer)
            {
                buffer.Dispose();
            }
        }

        s_allTempBuffers.Clear();
    }
}
