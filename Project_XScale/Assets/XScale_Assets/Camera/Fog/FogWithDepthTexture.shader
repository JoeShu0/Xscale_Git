Shader "Unlit/FogWithDepthTexture"
{
	Properties
	{
		_MainTex ("Base(RGB)", 2D) = "white" {}
		_FogDensity("FogDensity", Float) = 1.0
		_FogColor("FogColor",Color) = (1,1,1,1)
		_FogStart("FogStart", Float) = 0.0
		_FogEnd("FogEnd",Float) = 1.0
		_CameraDepth("CameraDepth",Float) = 0.1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"}
		LOD 100

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
			float _FogSmooth;

			half _AirFogDensity;
			fixed4 _AirFogColor;

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
				//UNITY_FOG_COORDS(1)
			};

			
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.uv_depth = v.uv;
			
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

				float LinearDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture , i.uv_depth));

				float LinearDepth_T = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth + float2(0, _FogSmooth)));
				float LinearDepth_L = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth + float2(-_FogSmooth, 0)));
				float LinearDepth_R = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth + float2(_FogSmooth, 0)));
				float LinearDepth_B = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth + float2(0, -_FogSmooth)));

				if (LinearDepth < LinearDepth_T)
					LinearDepth = LinearDepth_T;
				if (LinearDepth < LinearDepth_L)
					LinearDepth = LinearDepth_L;
				if (LinearDepth < LinearDepth_R)
					LinearDepth = LinearDepth_R;
				if (LinearDepth < LinearDepth_B)
					LinearDepth = LinearDepth_B;

				//LinearDepth = (LinearDepth + LinearDepth_T + LinearDepth_L + LinearDepth_R + LinearDepth_B) / 5;

				fixed4 finalColor = tex2D(_MainTex, i.uv);//取得本来的画面

				float3 worldPos = _WorldSpaceCameraPos + LinearDepth * i.interpolatedRay.xyz;
				float FogDistance = 0.0001;
				//float AirFogDistance = 0.001;
				if (_CameraDepth > 0 && worldPos.y < 0)
				{
					FogDistance = length(worldPos - _WorldSpaceCameraPos);
				}
				else if (_WorldSpaceCameraPos.y * worldPos.y < 0)
				{
					float3 ViewDir = _WorldSpaceCameraPos - worldPos;
					float3 SurfacePoint = worldPos + ViewDir * abs(worldPos.y / ViewDir.y);
					if (_CameraDepth > 0)
						FogDistance = length(SurfacePoint - _WorldSpaceCameraPos);
					//else
						//FogDistance = length(SurfacePoint - worldPos);
				}
				
				float fogDensity = exp(-_FogDensity * abs(FogDistance));
				//float AirFogDensity = exp(-_AirFogDensity * abs(AirFogDistance));
				//float fogDensity = (_FogEnd - worldPos.y) / (_FogEnd - _FogStart);
				//fogDensity = saturate(fogDensity * _FogDensity);
				/*
				if (_CameraDepth > 0)
					finalColor.rgb = lerp(finalColor.rgb / 3,finalColor.rgb, clamp((worldPos.y+15) / 10 - 1, 0, 1));
				finalColor.rgb = lerp(_FogColor.rgb, finalColor.rgb, fogDensity);
				*/
				
				return finalColor;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
