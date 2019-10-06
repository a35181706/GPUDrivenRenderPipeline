Shader "GPUDRP/MCR/Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        LOD 100
        Pass
        {
			Tags { "RenderType" = "Opaque" "LightMode" = "GPUDRPUnilt" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 5.0
            #include "UnityCG.cginc"
			#include "Assets/GPUDRP/Shader/MeshClusterRendering/MCRFunctions.cginc"
		  
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 normal : NORMAL;
				float4 uv0    : TexCoord0;
			};

			v2f vert(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                v2f o;
				VertexInfo vertex = getVertex(vertexID, instanceID);
				o.vertex = mul(UNITY_MATRIX_VP, DecodeWorldPos(vertex));
				o.normal = float4(DecodeWorldNormal(vertex), 1);
                o.uv0 = float4(DecodeUV0(vertex), 0, 1);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                return i.uv0;
            }
            ENDCG
        }
    }
}
