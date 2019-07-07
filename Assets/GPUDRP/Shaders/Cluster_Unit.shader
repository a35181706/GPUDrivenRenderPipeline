Shader "Hidden/Cluster_Unlit"
{
	SubShader
	{
		ZTest LEqual
		Cull off
		Tags {"RenderType" = "Opaque" "LightMode" = "GPUDRPUnlit" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5
			#include "UnityCG.cginc"
			
			#include "Assets/GPUDRP/Shaders/MCRVariables.cginc"
			#include "Assets/GPUDRP/Shaders/MCRFunctions.cginc"
			#include "Assets/GPUDRP/Shaders/ShaderVariables.cginc"
			#include "Assets/GPUDRP/Shaders/ShaderFunctions.cginc"
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 normal : NORMAL;
				float4 uv0    : TexCoord0;
				float4 textureIndex : TexCoord1;
			};

			v2f vert (uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
			{
				MCRVertex v = getVertex(vertexID, instanceID);
				float4 worldPos = float4(DecodeWorldPos(v), 1);
				v2f o;
				o.vertex = mul(UNITY_MATRIX_VP, worldPos);
				o.normal = float4(DecodeWorldNormal(v),1);
				o.uv0 = float4(DecodeUV0(v),0,1);
				o.textureIndex = float4(DecodeTextureIndex0_Float(v), 0, 0, 1);
				return o;
			}
			
			fixed4 frag (v2f i):SV_Target
			{
				return i.uv0;
			}
			ENDCG
		}
	}
}
