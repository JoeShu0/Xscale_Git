Shader "Custom/reflraction" 
{
	Properties
	{

		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map",2D) = "Bump"{}
		_CubeMap("CubeMap",Cube) = "_Skybox"{}
		_Distortion("Distortion", Range(0, 1000)) = 10
		_RefractionAmount("Refraction Amount",range(0,1)) = 10

	}
		SubShader
		{
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			LOD 200

			GrabPass{"_RefractionTex"}

			Pass
			{
			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma vertex vert
			#pragma fragment frag
			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			samplerCUBE _CubeMap;
			float _Distortion;
			fixed _RefractionAmount;
			sampler2D _RefractionTex;
			float4 _RefractionTex_TexelSize;

			struct a2v
			{
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
				fixed4 texcoord : TEXCOORD0;
				fixed4 tangent : TANGENT;
			};

			struct v2f
			{
				float4 pos : POSITION;
				fixed2 uv : TEXCOORD0;
				float3 wroldNormal : COLOR;
				fixed3 lightDir : TEXCOORD1;
				fixed4 TtoW0 : TEXCOORD2;
				fixed4 TtoW1 : TEXCOORD3;
				fixed4 TtoW2 : TEXCOORD4;
				//LIGHTING_COORDS(5, 6);
				float4 scrPos : TEXCOORD7;
				//UNITY_FOG_COORDS(7)//雾效UV
				//float4 ScreenUV : TEXCOORD8;//分配屏幕空间UV
		};

		v2f vert(a2v v) //顶点函数，在Domain函数中被调用，载入tesselation之后的顶点信息，返回世界坐标下的顶点信息，与切线空间到世界的旋转矩阵
		{
			v2f o;

			o.pos = UnityObjectToClipPos(v.vertex);
			o.wroldNormal = UnityObjectToWorldNormal(v.normal);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

			o.scrPos = ComputeGrabScreenPos(o.pos);//

			//为切线空间创建一个旋转矩阵
			TANGENT_SPACE_ROTATION;
			o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));

			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
			fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
			fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;
			// 构建从切线空间到世界的旋转矩阵
			o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
			o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
			o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

			//UNITY_TRANSFER_FOG(o, o.pos);
			//o.ScreenUV = ComputeGrabScreenPos(o.pos);// builtin function to get screen coordinates for tex2Dproj()

			// pass lighting information to pixel shader  
			//TRANSFER_VERTEX_TO_FRAGMENT(o);
			return o;
		}
		fixed4 frag(v2f i) : COLOR
		{
			fixed4 texColor = tex2D(_MainTex, i.uv);
			fixed3 norm = UnpackNormal(tex2D(_BumpMap, i.uv));
			float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
			fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
			fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));

			half3 worldNormal = normalize(half3(dot(i.TtoW0.xyz, norm), dot(i.TtoW1.xyz, norm), dot(i.TtoW2.xyz, norm)));

			fixed3 worldView = fixed3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
			fixed3 worldRefl = reflect(-worldViewDir, worldNormal);

			float2 offset = norm.xy * _Distortion * _RefractionTex_TexelSize.xy;
			i.scrPos.xy = i.scrPos.xy + offset;

			fixed3 refrCol = tex2D(_RefractionTex, i.scrPos.xy / i.scrPos.w).rgb;

			return fixed4(refrCol,1);
		}
		ENDCG
			}
		}

	FallBack "Diffuse"
}
