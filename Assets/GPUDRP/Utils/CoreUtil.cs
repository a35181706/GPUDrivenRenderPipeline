using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDRP
{
    public static class CoreUtil 
    {
        public static void Destroy(UnityEngine.Object obj)
        {
            if(!Application.isPlaying)
            {
                GameObject.DestroyImmediate(obj);
                return;
            }
            GameObject.Destroy(obj);
        }
    }
}

