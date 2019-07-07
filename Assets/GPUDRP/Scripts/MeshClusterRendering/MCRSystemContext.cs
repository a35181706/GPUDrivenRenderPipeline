using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUDrivenRenderPipeline
{

    public class MCRExecuterContext 
    {
        public ComputeBuffer clusterBuffer;         //ObjectInfo
        public ComputeBuffer instanceCountBuffer;   //uint
        public ComputeBuffer dispatchBuffer;
        public ComputeBuffer reCheckResult;
        public ComputeBuffer resultBuffer;          //uint
        public ComputeBuffer verticesBuffer;        //Point
        public ComputeBuffer reCheckCount;        //Point
        public ComputeBuffer moveCountBuffer;
        public int clusterCount;
    }

}
