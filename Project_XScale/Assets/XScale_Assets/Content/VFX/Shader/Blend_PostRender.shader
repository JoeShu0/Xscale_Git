Shader "XScale/Blend_PostRender"
{
	Properties
	{
		_Color("TintColor",Color) = (1,1,1,0)
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaMulti("AlphaMulti" ,range(0,2)) = 1
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "transparent" }
		LOD 100
			Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 Color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _AlphaMulti;
			fixed4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb = col.rgb * _Color.rgb;// *(_Color.a - 0.5);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				col.a = col.a * _AlphaMulti;
				return col;
			}
			ENDCG
		}
	}
}
