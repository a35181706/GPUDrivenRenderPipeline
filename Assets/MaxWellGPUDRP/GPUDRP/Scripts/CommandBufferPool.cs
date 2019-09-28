using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
namespace MaxWellGPUDrivenRenderPipeline
{
    public static class CommandBufferPool
    {
        private static Stack<CommandBuffer> m_bufferCache = new Stack<CommandBuffer>();
        private static string KeyCmdKey = "%OnCache%";
        public static CommandBuffer Get(string name = "")
        {
            CommandBuffer buffer = null;
            if (m_bufferCache.Count > 0)
            {
                buffer = m_bufferCache.Pop();
            }
            else
            {
                buffer = new CommandBuffer();
            }
            buffer.Clear();
            buffer.name = name;
            return buffer;
        }

        public static void Release(CommandBuffer buffer)
        {
            if (null != buffer)
            {
                if(buffer.name.Equals(KeyCmdKey))
                {
                    return;
                }
                buffer.Clear();
                buffer.name = KeyCmdKey;
                m_bufferCache.Push(buffer);
            }
        }
    }
}