using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.IO;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Jobs;
using System;
namespace GPUDrivenRenderPipeline
{
    /// <summary>
    /// MeshClusterRendering生成器
    /// </summary>
    public unsafe static class MCRGenerator
    {
        struct Triangle
        {
            public MCRVertex a;
            public MCRVertex b;
            public MCRVertex c;
            public Triangle* last;
            public Triangle* next;
        }

        struct Voxel
        {
            public Triangle* start;
            public int count;
            public void Add(Triangle* ptr)
            {
                if (start != null)
                {
                    start->last = ptr;
                    ptr->next = start;
                }
                start = ptr;
                count++;
            }
            public Triangle* Pop()
            {
                if (start->next != null)
                {
                    start->next->last = null;
                }
                Triangle* last = start;
                start = start->next;
                count--;
                return last;
            }
        }

        public static int GenerateCluster(NativeList<MCRVertex> vertexFromMesh, NativeList<int> triangles, Bounds bd, string fileName, int voxelCount, int sceneCount)
        {
            NativeList<MCRCluster> boxes;
            NativeList<MCRVertex> points;
            GetCluster(vertexFromMesh, triangles, bd, out boxes, out points, voxelCount);
            
            string filenameWithExtent = fileName + ".mpipe";
            byte[] bytes = new byte[boxes.Length * sizeof(MCRCluster)];
            for (int i = 0; i < boxes.Length; ++i)
            {
                boxes[i].index = sceneCount;
            }
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(global::GPUDrivenRenderPipeline.UnsafeUtility.Ptr<byte>(bytes), boxes.unsafePtr, bytes.Length);
            File.WriteAllBytes("Assets/BinaryData/MapInfos/" + filenameWithExtent, bytes);
            bytes = new byte[points.Length * sizeof(MCRVertex)];
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(global::GPUDrivenRenderPipeline.UnsafeUtility.Ptr<byte>(bytes), points.unsafePtr, bytes.Length);
            File.WriteAllBytes("Assets/BinaryData/MapPoints/" + filenameWithExtent, bytes);
            //Dispose Native Array
            return boxes.Length;
        }

        public static void GetCluster(NativeList<MCRVertex> vertexFromMesh, NativeList<int> triangles, Bounds bd, out NativeList<MCRCluster> boxes, out NativeList<MCRVertex> points, int voxelCount)
        {
            NativeList<Triangle> trs = GenerateTriangle(triangles, vertexFromMesh);
            Voxel[,,] voxels = GetVoxelData(trs, voxelCount, bd);
            GetClusterFromVoxel(voxels, out boxes, out points, triangles.Length, voxelCount);
        }

        private static NativeList<Triangle> GenerateTriangle(NativeList<int> triangles, NativeList<MCRVertex> vertexFromMeshs)
        {
            NativeList<Triangle> retValue = new NativeList<Triangle>(triangles.Length / 3, Allocator.Temp);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Triangle tri = new Triangle
                {
                    a = vertexFromMeshs[triangles[i]],
                    b = vertexFromMeshs[triangles[i + 1]],
                    c = vertexFromMeshs[triangles[i + 2]],
                    last = null,
                    next = null
                };
                retValue.Add(tri);
            }
            return retValue;
        }

        private static Voxel[,,] GetVoxelData(NativeList<Triangle> trianglesFromMesh, int voxelCount, Bounds bound)
        {
            Voxel[,,] voxels = new Voxel[voxelCount, voxelCount, voxelCount];
            for (int x = 0; x < voxelCount; ++x)
                for (int y = 0; y < voxelCount; ++y)
                    for (int z = 0; z < voxelCount; ++z)
                    {
                        voxels[x, y, z] = new Voxel();
                    }
            float3 downPoint = bound.center - bound.extents;
            for (int i = 0; i < trianglesFromMesh.Length; ++i)
            {
                ref Triangle tr = ref trianglesFromMesh[i];
                float3 position = (tr.a.position + tr.b.position + tr.c.position) / 3;
                float3 localPos = saturate((position - downPoint) / bound.size);
                int3 coord = (int3)(localPos * voxelCount);
                coord = min(coord, voxelCount - 1);
                voxels[coord.x, coord.y, coord.z].Add((Triangle*)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf(ref tr));
            }
            return voxels;
        }

        private static void GetClusterFromVoxel(Voxel[,,] voxels, out NativeList<MCRCluster> Clusteres, out NativeList<MCRVertex> points, int vertexCount, int voxelSize)
        {
            int3 voxelCoord = 0;
            float3 lessPoint = float.MaxValue;
            float3 morePoint = float.MinValue;
            int clusterCount = Mathf.CeilToInt((float)vertexCount / MCRConstant.c_CLUSTER_VERTEX_COUNT);
            points = new NativeList<MCRVertex>(clusterCount * MCRConstant.c_CLUSTER_VERTEX_COUNT, Allocator.Temp);
            Clusteres = new NativeList<MCRCluster>(clusterCount, Allocator.Temp);
            //Collect all full
            for (int i = 0; i < clusterCount - 1; ++i)
            {
                NativeList<MCRVertex> currentPoints = new NativeList<MCRVertex>(MCRConstant.c_CLUSTER_VERTEX_COUNT, Allocator.Temp);
                int lastedVertex = MCRConstant.c_CLUSTER_VERTEX_COUNT / 3;
                ref Voxel currentVoxel = ref voxels[voxelCoord.x, voxelCoord.y, voxelCoord.z];
                int loopStart = min(currentVoxel.count, max(lastedVertex - currentVoxel.count, 0));
                for (int j = 0; j < loopStart; j++)
                {
                    Triangle* tri = currentVoxel.Pop();
                    currentPoints.Add(tri->a);
                    currentPoints.Add(tri->b);
                    currentPoints.Add(tri->c);
                }
                lastedVertex -= loopStart;

                for (int size = 1; lastedVertex > 0; size++)
                {
                    int3 leftDown = max(voxelCoord - size, 0);
                    int3 rightUp = min(voxelSize, voxelCoord + size);
                    for (int x = leftDown.x; x < rightUp.x; ++x)
                        for (int y = leftDown.y; y < rightUp.y; ++y)
                            for (int z = leftDown.z; z < rightUp.z; ++z)
                            {
                                ref Voxel vxl = ref voxels[x, y, z];
                                int vxlCount = vxl.count;
                                for (int j = 0; j < vxlCount; ++j)
                                {
                                    voxelCoord = int3(x, y, z);
                                    Triangle* tri = vxl.Pop();
                                    //   try
                                    // {
                                    currentPoints.Add(tri->a);
                                    currentPoints.Add(tri->b);
                                    currentPoints.Add(tri->c);
                                    /* }
                                     catch
                                     {
                                         Debug.Log(vxlCount);
                                         Debug.Log(tri->a);
                                         Debug.Log(tri->b);
                                         Debug.Log(tri->c);
                                         Debug.Log(currentPoints.Length);
                                         return;
                                     }*/
                                    lastedVertex--;
                                    if (lastedVertex <= 0) goto CONTINUE;
                                }
                            }

                }
            CONTINUE:
                points.AddRange(currentPoints);
                lessPoint = float.MaxValue;
                morePoint = float.MinValue;
                foreach (var j in currentPoints)
                {
                    if (j.position.x < lessPoint.x) lessPoint.x = j.position.x;
                    else if (j.position.x > morePoint.x) morePoint.x = j.position.x;
                    if (j.position.y < lessPoint.y) lessPoint.y = j.position.y;
                    else if (j.position.y > morePoint.y) morePoint.y = j.position.y;
                    if (j.position.z < lessPoint.z) lessPoint.z = j.position.z;
                    else if (j.position.z > morePoint.z) morePoint.z = j.position.z;
                }
                MCRCluster cb = new MCRCluster
                {
                    extent = (morePoint - lessPoint) / 2,
                    position = (morePoint + lessPoint) / 2
                };
                Clusteres.Add(cb);
                currentPoints.Dispose();
            }
            //Collect and degenerate
            NativeList<MCRVertex> leftedPoints = new NativeList<MCRVertex>(MCRConstant.c_CLUSTER_VERTEX_COUNT, Allocator.Temp);
            for (int x = 0; x < voxelSize; ++x)
                for (int y = 0; y < voxelSize; ++y)
                    for (int z = 0; z < voxelSize; ++z)
                    {
                        ref Voxel vxl = ref voxels[x, y, z];
                        int vxlCount = vxl.count;
                        for (int j = 0; j < vxlCount; ++j)
                        {
                            Triangle* tri = vxl.Pop();
                            leftedPoints.Add(tri->a);
                            leftedPoints.Add(tri->b);
                            leftedPoints.Add(tri->c);

                        }
                    }
            if (leftedPoints.Length <= 0) return;
            lessPoint = float.MaxValue;
            morePoint = float.MinValue;
            foreach (var j in leftedPoints)
            {
                if (j.position.x < lessPoint.x) lessPoint.x = j.position.x;
                else if (j.position.x > morePoint.x) morePoint.x = j.position.x;
                if (j.position.y < lessPoint.y) lessPoint.y = j.position.y;
                else if (j.position.y > morePoint.y) morePoint.y = j.position.y;
                if (j.position.z < lessPoint.z) lessPoint.z = j.position.z;
                else if (j.position.z > morePoint.z) morePoint.z = j.position.z;
            }
            MCRCluster lastBox = new MCRCluster
            {
                extent = (morePoint - lessPoint) / 2,
                position = (morePoint + lessPoint) / 2
            };
            Clusteres.Add(lastBox);
            for (int i = leftedPoints.Length; i < MCRConstant.c_CLUSTER_VERTEX_COUNT; i++)
            {
                leftedPoints.Add(new MCRVertex());
            }
            points.AddRange(leftedPoints);
        }
    }
}

