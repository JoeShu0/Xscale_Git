Shader "Custom/GenericShader_WithFog" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "gray" {}
		_NormalTex("NormalMap", 2D) = "bump" {}
		_SpecularTex("SpecularMap", 2D) = "gray" {}
		_SmoothnessTex("SmoothnessMap", 2D) = "gray" {}
		_EmissionTex("EmissionMap", 2D) = "black" {}
		_EmissionIntensity("EmissionIntensity", float) = 0

		_CausticsTex("CausticsMap",2D) = "black" {}
		_CausticsDrift("CausticsDiftDir",Vector) = (1,1,1,1)
		_CausticsScale("CausticsScale",Range(0.1,10)) = 2 
		_CausticsFadeDistance("CausticsFadeDistance",Vector) = (15,100,0,0)
		_CausticsFactor("CausticsFactor",Range(0,2)) = 1

		_AirFogColorDensity("AirFogColorDensity",Color) = (0,0,0,0)
		_WaterFogColorDensity("WaterFogColorDensity",Color) = (0,0,0,0)
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf StandardSpecular fullforwardshadows finalcolor:mycolor 
			//#pragma finalcolor:mycolor vertex:myvert
			#pragma multi_compile_fog

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			fixed4 _Color;
			sampler2D _MainTex;
			sampler2D _NormalTex;
			sampler2D _SpecularTex;
			sampler2D _SmoothnessTex;
			sampler2D _EmissionTex;
			float _EmissionIntensity;

			sampler2D _CausticsTex;

			float4x4 _CausticsMatrix;
			float3 _CausticsDrift;
			float2 _CausticsFadeDistance;
			float _CausticsScale;
			float _CausticsFactor;
			float4 _AirFogColorDensity;
			float4 _WaterFogColorDensity;
			float _MainCameraHeight;

			struct Input {
				float2 uv_MainTex;
				float2 uv_NormalTex;
				float2 uv_SpecularTex;
				float2 uv_SmoothnessTex;
				float2 uv_EmissionTex;
				float2 uv_CausticsTex;
				
				//float WaterFogValue;

				float3 worldPos;
				float3 worldNormal;
				INTERNAL_DATA
			};

			void mycolor(Input IN, SurfaceOutputStandardSpecular o, inout fixed4 color)
			{
				float Length = length(IN.worldPos - _WorldSpaceCameraPos);
				float AirLength = 0.00001;
				float WaterLength = 0.0001;
				if (IN.worldPos.y * _WorldSpaceCameraPos.y < 0)
				{
					if (_WorldSpaceCameraPos.y < 0)
					{
						AirLength = Length * abs(IN.worldPos.y / (IN.worldPos - _WorldSpaceCameraPos).y);
						WaterLength = Length - AirLength;
					}
					else
					{
						AirLength = Length * abs(_WorldSpaceCameraPos.y / (_WorldSpaceCameraPos - IN.worldPos).y);
						WaterLength = Length - AirLength;
					}
				}
				else if (_WorldSpaceCameraPos.y < 0)
					WaterLength = Length;
				else if (_WorldSpaceCameraPos.y > 0)
					AirLength = Length;
				float AirFogDensity = exp(-_AirFogColorDensity.a/10 * abs(AirLength));
				float WaterFogDensity = exp(-_WaterFogColorDensity.a / 10 * abs(WaterLength));
				
				if (_WorldSpaceCameraPos.y * _MainCameraHeight < 0)
				{
					if (_WorldSpaceCameraPos.y < 0)
						WaterFogDensity = 1;
					if (_WorldSpaceCameraPos.y > 0)
						AirFogDensity = 1;
				}
				color = lerp(color , _WaterFogColorDensity , 1 - WaterFogDensity);
				color = lerp(color, _AirFogColorDensity, 1 - AirFogDensity);
			}

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandardSpecular o) {
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				fixed3 n = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));
				fixed3 sp = tex2D(_SpecularTex, IN.uv_SpecularTex);
				fixed3 sm = tex2D(_SmoothnessTex, IN.uv_SmoothnessTex);


				//c.rgb += Emission(causticsTextureCoord) * fadeFactor * belowFactor;


				fixed3 em = tex2D(_EmissionTex, IN.uv_EmissionTex);

				o.Alpha = c.a;
				o.Normal = n;
				o.Specular = sp;
				o.Smoothness = sm;
				o.Emission = em * _EmissionIntensity;

				if (IN.worldPos.y < 0)
				{
					float3 drift = _CausticsDrift.xyz * _Time.y;
					float fadeFactor = min(max(0, 1 + (IN.worldPos.y / _CausticsFadeDistance.y)) , min(1.0f, -IN.worldPos.y / _CausticsFadeDistance.x));

					float3 upVec = float3(0, 1, 0);
					float belowFactor = max(0, dot(normalize(WorldNormalVector(IN, o.Normal)), upVec) + 0.5);
					float3 worldCoord = (IN.worldPos + drift) / _CausticsScale;

					_CausticsMatrix = float4x4 (
						-0.48507, -0.56592, 0.66667, 0.00000,
						0.72761, 0.16169, 0.66667, 0.00000,
						-0.48507, 0.80845, 0.33333, 0.00000,
						0.00000, 0.00000, 0.00000, 1.00000
						);

					float2 causticsTextureCoord = mul(float4 (worldCoord, 1), _CausticsMatrix).xy;
					fixed3 ca = tex2D(_CausticsTex, causticsTextureCoord) * belowFactor *fadeFactor *_CausticsFactor;
					c.rgb += ca;
				}
				o.Albedo = c.rgb;

			}
			ENDCG
	}
	
	FallBack "Diffuse"
}
