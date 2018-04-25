Shader "Unlit/FogWithDepthForRef"
{
	Properties
	{
		_MainTex ("Base(RGB)", 2D) = "white" {}
		_FogDensity("FogDensity", Float) = 2.8
		_FogColor("FogColor",Color) = (1,0,0,1)

		_CameraDepth("CameraDepth",Float) = 0.1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		ZTest Always
			Cull off 
			ZWrite off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _MainTex_TexelSize;

			float4x4 _FrustumCornersRay;

			sampler2D _CameraDepthTexture;
			half _FogDensity;
			fixed4 _FogColor;
			float _FogStart;
			float _FogEnd;
			float _CameraDepth;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv_depth : TEXCOORD1;
				float4 interpolatedRay : TEXCOORD2;
				float4 ScreenPos :TEXCOORD3;
				//UNITY_FOG_COORDS(1)
			};

			
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.uv_depth = v.uv;
				o.ScreenPos = ComputeGrabScreenPos(o.pos);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv_depth.y = 1 - o.uv_depth.y;
				#endif

				int index = 0;
				if (v.uv.x < 0.5 && v.uv.y < 0.5)
					index = 0;
				else if (v.uv.x > 0.5 && v.uv.y < 0.5)
					index = 1;
				else if (v.uv.x > 0.5 && v.uv.y > 0.5)
					index = 2;
				else 
					index = 3;

				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					index = 3 - index;
				#endif

				o.interpolatedRay = _FrustumCornersRay[index];

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

				float LinearDepth = tex2D(_CameraDepthTexture , (i.ScreenPos.xy) / i.ScreenPos.w).r;
				//float LinearDepth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, float4((i.ScreenPos.xy) / i.ScreenPos.w, 1, 1))));
				float3 refCamPos = float3(_WorldSpaceCameraPos.x, _WorldSpaceCameraPos.y, _WorldSpaceCameraPos.z);
				float3 worldPos = refCamPos + LinearDepth * i.interpolatedRay.xyz;
				float FogDistance = 0.0001;
				float fogDensity = 0;

				float3 ViewDir = refCamPos - worldPos;
				float3 SurfacePoint = worldPos + ViewDir * abs(worldPos.y / ViewDir.y);
				FogDistance = length(refCamPos - worldPos);
				if (refCamPos.y > 0)
				{
					fogDensity = exp(-_FogDensity * abs(FogDistance))*0.9;
				}
				else
				{
					fogDensity = exp(-_FogDensity * abs(FogDistance))*0.8;
					//fogDensity = lerp(0, exp(-_FogDensity * abs(FogDistance)), clamp(refCamPos.y, 0, 20)/20);
				}
				
				
				
				
				//float fogDensity = exp(-_FogDensity * abs(LinearDepth*10));
				//float fogDensity = (_FogEnd - worldPos.y) / (_FogEnd - _FogStart);
				//fogDensity = saturate(fogDensity * _FogDensity);
				fixed4 finalColor = tex2D(_MainTex, i.uv);
				finalColor.rgb = lerp(_FogColor.rgb, finalColor.rgb,1- fogDensity);
				//finalColor =float4(LinearDepth, LinearDepth, LinearDepth,1);
				//finalColor.rgb = lerp(_FogColor.rgb, finalColor.rgb, fogDensity);
				//return  float4(FogDistance, FogDistance, FogDistance, 1);
				//return float4(worldPos,1);
				return finalColor;
			}
			ENDCG
		}
	}
	FallBack off
}
