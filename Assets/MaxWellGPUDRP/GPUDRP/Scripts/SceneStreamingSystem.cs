using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using System.IO;

namespace MaxWellGPUDrivenRenderPipeline
{
    public unsafe sealed class SceneStreamingSysetm 
    {
        public static bool loading = false;
        public enum State
        {
            Unloaded, Loaded, Loading
        }
        public State state;
        private NativeArray<MCRCluster> clusterBuffer;
        private NativeArray<MCRVertex> vertexBuffer;

        private static Action<object> generateAsyncFunc = (obj) =>
        {
            SceneStreamingSysetm str = obj as SceneStreamingSysetm;
            str.GenerateAsync();
        };

        private MCRExecuterContext mcrContext = null;
        MCRClusterInfo property;
        int propertyCount;
        public SceneStreamingSysetm(MCRClusterInfo property, int propertyCount,MCRExecuterContext context)
        {
            this.propertyCount = propertyCount;
            state = State.Unloaded;
            this.property = property;
            mcrContext = context;

            clusterBuffer = new NativeArray<MCRCluster>(property.clusterCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            vertexBuffer = new NativeArray<MCRVertex>(property.clusterCount * MCRConstant.c_CLUSTER_VERTEX_COUNT, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }
        static string[] allStrings = new string[3];
        private static byte[] bytesArray = new byte[8192];
        private static byte[] GetByteArray(int length)
        {
            if (bytesArray == null || bytesArray.Length < length)
            {
                bytesArray = new byte[length];
            }
            return bytesArray;
        }
        public void GenerateAsync(bool listCommand = true)
        {

            MCRCluster* clusterData = clusterBuffer.Ptr();
            MCRVertex* verticesData = vertexBuffer.Ptr();
            const string infosPath = "Assets/BinaryData/MapInfos/";
            const string pointsPath = "Assets/BinaryData/MapPoints/";
            UnSafeStringBuilder sb = new UnSafeStringBuilder(pointsPath.Length + property.name.Length + ".mpipe".Length);
            allStrings[0] = infosPath;
            allStrings[1] = property.name;
            allStrings[2] = ".mpipe";
            sb.Combine(allStrings);
            // FileStream fileStream = new FileStream(sb.str, FileMode.Open, FileAccess.Read);
            using (FileStream reader = new FileStream(sb.str, FileMode.Open, FileAccess.Read))
            {
                int length = (int)reader.Length;
                byte[] bytes = GetByteArray(length);
                reader.Read(bytes, 0, length);
                fixed (byte* b = bytes)
                {
                    Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(clusterData, b, length);
                }
            }
            allStrings[0] = pointsPath;
            sb.Combine(allStrings);
            using (FileStream reader = new FileStream(sb.str, FileMode.Open, FileAccess.Read))
            {
                int length = (int)reader.Length;
                byte[] bytes = GetByteArray(length);
                reader.Read(bytes, 0, length);
                fixed (byte* b = bytes)
                {
                    Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(verticesData, b, length);
                }
            }
            LoadingCommandQueue commandQueue = LoadingThread.commandQueue;

            if (listCommand)
            {
                lock (commandQueue)
                {
                    commandQueue.Queue(GenerateRun());
                }
            }
        }

        public IEnumerator Generate()
        {
            if (state == State.Unloaded)
            {
                state = State.Loading;
                while (loading)
                {
                    yield return null;
                }
                loading = true;
                LoadingThread.AddCommand(generateAsyncFunc, this);
            }
        }

        public IEnumerator Delete()
        {
            if (state == State.Loaded)
            {
                state = State.Loading;
                while (loading)
                {
                    yield return null;
                }
                loading = true;
                DeleteRun();
            }
        }

        #region MainThreadCommand
        private const int MAXIMUMINTCOUNT = 5000;
        private const int MAXIMUMVERTCOUNT = 100;

        public void DeleteRun()
        { 
            int result = mcrContext.clusterCount - property.clusterCount;
            ComputeShader shader = MaxWellGPUDrivenRenderPipelineAssets.Instance.streamingShader;
            if (result > 0)
            {
                NativeArray<int> indirectArgs = new NativeArray<int>(5, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                indirectArgs[0] = 0;
                indirectArgs[1] = 1;
                indirectArgs[2] = 1;
                indirectArgs[3] = result;
                indirectArgs[4] = propertyCount;
                mcrContext.moveCountBuffer.SetData(indirectArgs);
                ComputeBuffer indexBuffer = ComputeBufferPool.GetTempBuffer(property.clusterCount, 8);
                indirectArgs.Dispose();
                shader.SetBuffer(0, ShaderID.instanceCountBuffer, mcrContext.moveCountBuffer);
                shader.SetBuffer(0, ShaderID.clusterBuffer, mcrContext.clusterBuffer);
                shader.SetBuffer(0, ShaderID._IndexBuffer, indexBuffer);
                shader.SetBuffer(1, ShaderID._IndexBuffer, indexBuffer);
                shader.SetBuffer(1, ShaderID.verticesBuffer, mcrContext.verticesBuffer);

                ComputeShaderUtility.Dispatch(shader, 0, result);
                shader.DispatchIndirect(1, mcrContext.moveCountBuffer);
            }
            mcrContext.clusterCount = result;
            loading = false;
            state = State.Unloaded;
        }

        private IEnumerator GenerateRun()
        {
            int targetCount;
            int currentCount = 0;
            while ((targetCount = currentCount + MAXIMUMVERTCOUNT) < clusterBuffer.Length)
            {
                mcrContext.clusterBuffer.SetData(clusterBuffer, currentCount, currentCount + mcrContext.clusterCount, MAXIMUMVERTCOUNT);
                mcrContext.verticesBuffer.SetData(vertexBuffer, currentCount * MCRConstant.c_CLUSTER_VERTEX_COUNT, (currentCount + mcrContext.clusterCount) * MCRConstant.c_CLUSTER_VERTEX_COUNT, MAXIMUMVERTCOUNT * MCRConstant.c_CLUSTER_VERTEX_COUNT);
                currentCount = targetCount;
                yield return null;
            }

            //TODO
            mcrContext.clusterBuffer.SetData(clusterBuffer, currentCount, currentCount + mcrContext.clusterCount, clusterBuffer.Length - currentCount);
            mcrContext.verticesBuffer.SetData(vertexBuffer, currentCount * MCRConstant.c_CLUSTER_VERTEX_COUNT, (currentCount + mcrContext.clusterCount) * MCRConstant.c_CLUSTER_VERTEX_COUNT, (clusterBuffer.Length - currentCount) * MCRConstant.c_CLUSTER_VERTEX_COUNT);
            int clusterCount = clusterBuffer.Length;
            clusterBuffer.Dispose();
            vertexBuffer.Dispose();
            loading = false;
            state = State.Loaded;
            mcrContext.clusterCount += clusterCount;
            Debug.Log("Loaded");
        }
        #endregion
    }
}

