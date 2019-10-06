using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.InteropServices;

public static class IOUtils 
{
    public static void DeleteFile(string filePath)
    {
        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// 将字节数组转换为结构体
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static T ByteToStruct<T>(byte[] bytes) where T : struct
    {
        Type t = typeof(T);
        //得到结构体大小
        int size = Marshal.SizeOf(t);

        if (size > bytes.Length)
            return default(T);
        //分配结构大小的内存空间
        IntPtr structPtr = Marshal.AllocHGlobal(size);
        //将BYTE数组拷贝到分配好的内存空间
        Marshal.Copy(bytes, 0, structPtr, size);
        //将内存空间转换为目标结构
        T obj = (T)Marshal.PtrToStructure(structPtr, t);
        //释放内容空间
        Marshal.FreeHGlobal(structPtr);
        return obj;
    }
    /// <summary>
    /// 将结构转换为字节数组
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static byte[] StructToByte<T>(T obj) where T : struct
    {
        int size = Marshal.SizeOf(obj);
        //创建byte数组
        byte[] bytes = new byte[size];
        IntPtr structPtr = Marshal.AllocHGlobal(size);
        //将结构体拷贝到分配好的内存空间
        Marshal.StructureToPtr(obj, structPtr, false);
        //从内存空间拷贝到byte数组
        Marshal.Copy(structPtr, bytes, 0, size);
        //释放内存空间
        Marshal.FreeHGlobal(structPtr);
        return bytes;
    }
}
