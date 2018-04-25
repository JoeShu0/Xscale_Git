// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/WaterShader_Test04" 
{
	Properties
	{
		_Tessellation("Tessellation", range(1,256)) = 40
	
		_MinDist("MinDistance",range(1,50)) = 10
		_MaxDist("MaxDistance",range(15,100)) = 25
		_Displacement("Displacement", Range(0, 1)) = 0.1
		
		_Smooth("NormalSmoothFactor",range(0.1,10)) = 1

		_PlaneSize("PlaneSize",range(1,100)) = 10//Do not use Unless Needed


		_DispTex("MainNoiseTexture", 2D) = "gray" {}
		_MainTile01("MainTile01",range(1,100)) = 1
		_MovingSpeed("MovingSpeed",range(1,100)) = 10
		_MovingDirection("MovingDirection",range(0,360)) = 0

		_DispTex01("SubNoiseTexture01", 2D) = "gray" {}
		_SubTile01("SubTile01",range(1,100)) = 1
		_SubMS01("SubMovingSpeed01",range(1,100)) = 0
		_SubMD01("SubMovingDirection01",range(0,360)) = 0
		_SubSt01("SubNoiseStrength01",range(0,1)) = 0.1

		_DispTex02("SubNoiseTexture02", 2D) = "gray" {}
		_SubTile02("SubTile02",range(1,100)) = 1
		_SubMS02("SubMovingSpeed02",range(1,100)) = 0
		_SubMD02("SubMovingDirection02",range(0,360)) = 0
		_SubSt02("SubNoiseStrength02",range(0,1)) = 0.1

		_Color("BaseColor", Color) = (1,1,1,1)
		_TransParent("Transparent", range(0,1)) = 0.5
		_MainTex("Albedo (RGB)", 2D) = "white" {}//
		//_Glossiness("Smoothness", Range(0,1)) = 0.5
		//_Metallic("Metallic", Range(0,1)) = 0.0
		_NormalMap("Normalmap", 2D) = "bump" {}
		_NormalAddFactor("NormalAddFactor",range(0,1))=0.1

		_ReflColor("ReflectiveColor", Color) = (1,1,1,1)
		_ReflecTexture("ReflecTexture", 2D) = "gray" {}
		_ReflecDistortion("ReflecDistortion", Range(0, 10000)) = 10
		//_Cube("Reflection Cubemap", Cube) = "black" {}
		_CriticalAngle("CosTotalReflectionAngle",range(0,1)) = 0.7 //完全反射角的Cos值
		_RefracDistortion("RefracDistortion", Range(0, 10000)) = 10//折射偏移程度
		_RefractionAmount("RefractionBaseAmount",range(0,1)) = 0.5//折射比例
		//_RefrMaxDistance("RefrMaxDistance",range(100,1000)) = 500
	
		//_WaterFogColor("FogColor",Color) = (1,1,1,1)
		_Specular("Specular", Range(1.0, 500.0)) = 250.0
		_Gloss("Gloss", Range(0.0, 1.0)) = 0.2
		//_FresnelScale("FresnelScale",range(0,1)) = 0.5

		//_IsBackFace("IsBackFace",int) = 0
		//水面下视时的fog参数
		_FogDensity("FogDensity", Float) = 1.0
		_FogColor("FogColor",Color) = (1,1,1,1)
		_CameraDepth("CameraDepth",Float) = 0.1
		//_MainTex("Base(RGB)", 2D) = "white" {}

	
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200

		ZWrite off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull off

		GrabPass{ "_RefractionTex" }//抓取一次之前渲染到的图像

		Pass{
		Tags{ "LightMode" = "ForwardBase" }

		CGPROGRAM
#pragma hull hull
#pragma domain domain
#pragma vertex tessvert
#pragma fragment frag
#define UNITY_PASS_FORWARDBASE
#pragma multi_compile_fog
#pragma multi_compile_fwdbase_fullshadows
#pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
#pragma target 5.0
#include "UnityCG.cginc"
#include "Tessellation.cginc"
#include "Lighting.cginc"  
#include "AutoLight.cginc"  

		sampler2D _MainTex;
		float4 _MainTex_ST;
		half4 _MainTex_TexelSize;
		fixed4 _Color;
		float _TransParent;
		sampler2D _NormalMap;
		float _NormalAddFactor;
		float _Specular;
		float _Gloss;
		//fixed4 _WaterFogColor;
		//float _FresnelScale;

		float _Tessellation;
		float _Distance;
		float _MinDist;
		float _MaxDist;
		float _Displacement;

		sampler2D _DispTex;
		sampler2D _DispTex01;
		sampler2D _DispTex02;
		

		float _SubSt01, _SubSt02, _MainTile01;
		float _SubTile01, _SubTile02;
		float _SubMS01, _SubMS02;
		
		float _PlaneSize;
		float _MovingSpeed;
		float _Smooth;

		//samplerCUBE _Cube;
		fixed4 _ReflColor;
		sampler2D _ReflecTexture;
		float4 _ReflecTexture_TexelSize;
		float _ReflecDistortion;

		float _CriticalAngle;
		float _RefracDistortion;
		fixed _RefractionAmount;

		sampler2D _CameraDepthTexture;
		sampler2D _RefractionTex;
		float4 _RefractionTex_TexelSize;//抓取到图像的像素尺寸
		//sampler2D _CameraDepthTexture;//相机深度图
		float _RefrMaxDistance;

		//int _IsBackFace;

		float4x4 _FrustumCornersRay;//摄像机的投射矩阵
		half _FogDensity;
		fixed4 _FogColor;
		float _CameraDepth;
		



	struct a2v {
		float4 vertex : POSITION;
		fixed3 normal : NORMAL;
		fixed4 texcoord : TEXCOORD0;
		fixed4 tangent : TANGENT;
	};

	struct v2f {
		float4 pos : POSITION;
		fixed2 uv : TEXCOORD0;
		float3 wroldNormal : COLOR;
		fixed3 lightDir : TEXCOORD1;
		fixed4 TtoW0 : TEXCOORD2;
		fixed4 TtoW1 : TEXCOORD3;
		fixed4 TtoW2 : TEXCOORD4;
		UNITY_FOG_COORDS(5)
		float4 interpolatedRay : TEXCOORD6;
		float4 ScreenPos : TEXCOORD7;//分配屏幕空间位置
		float2 ScreenDepth : TEXCOORD8;//分配屏幕空间Depth
	};

	v2f vert(a2v v) //顶点函数，在Domain函数中被调用，载入tesselation之后的顶点信息，返回世界坐标下的顶点信息，与切线空间到世界的旋转矩阵
	{
		v2f o;

		o.pos = UnityObjectToClipPos(v.vertex);
		o.wroldNormal = UnityObjectToWorldNormal(v.normal);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

		UNITY_TRANSFER_DEPTH(o.ScreenDepth);//写入深度值
		UNITY_TRANSFER_FOG(o, o.pos);

		o.ScreenPos = ComputeGrabScreenPos(o.pos);//计算屏幕空间的抓取到的坐标

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

		//水面下视时的fog参数计算<<<<<<
#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;
#endif

		int index = 0;
		if (v.texcoord.x < 0.5 && v.texcoord.y < 0.5)
			index = 0;
		else if (v.texcoord.x > 0.5 && v.texcoord.y < 0.5)
			index = 1;
		else if (v.texcoord.x > 0.5 && v.texcoord.y > 0.5)
			index = 2;
		else
			index = 3;
		index = 0;

#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			index = 3 - index;
#endif

		o.interpolatedRay = _FrustumCornersRay[index];
		//水面下视时的fog参数计算End<<<<<< 


		TRANSFER_VERTEX_TO_FRAGMENT(o);
		return o;
	}

	float4 GetVertNewPos(float4 p)//根据贴图采样生成动态波浪，顶点位置计算函数，domain函数中被调用
	{
		float C_time = _Time.y;
		//float C_time = 10;

		float Mainoffset = _MovingSpeed * C_time * 0.001;
		float Suboffset01 = _SubMS01 * C_time * 0.001;
		float Suboffset02 = _SubMS02 * C_time * 0.001;

		float Md = (tex2Dlod(_DispTex, float4(p.x / _PlaneSize*_MainTile01 + 0.5 + Mainoffset, p.z / _PlaneSize*_MainTile01 + 0.5 + Mainoffset, 0, 0)).r) * _Displacement;
		//Smoothing?
		float SMxup = tex2Dlod(_DispTex, float4((p.x + 0.2) / _PlaneSize*_MainTile01 + 0.5 + Mainoffset, p.z / _PlaneSize*_MainTile01 + 0.5 + Mainoffset, 0, 0)).r * _Displacement;
		float SMxdw = tex2Dlod(_DispTex, float4((p.x - 0.2) / _PlaneSize*_MainTile01 + 0.5 + Mainoffset, p.z / _PlaneSize*_MainTile01 + 0.5 + Mainoffset, 0, 0)).r * _Displacement;
		float SMzup = tex2Dlod(_DispTex, float4((p.x) / _PlaneSize*_MainTile01 + 0.5 + Mainoffset, (p.z + 0.2) / _PlaneSize*_MainTile01 + 0.5 + Mainoffset, 0, 0)).r * _Displacement;
		float SMzdw = tex2Dlod(_DispTex, float4((p.x) / _PlaneSize*_MainTile01 + 0.5 + Mainoffset, (p.z - 0.2) / _PlaneSize*_MainTile01 + 0.5 + Mainoffset, 0, 0)).r * _Displacement;

		float SMD = (SMxup + SMxdw + SMzup + SMzdw) / 4;

		float Sd01 = (tex2Dlod(_DispTex01, float4(p.x / _PlaneSize*_SubTile01 + 0.5 + Suboffset01, p.z / _PlaneSize*_SubTile01 + 0.5 + Suboffset01, 0, 0)).r - 0.5) * _Displacement;
		float Sd02 = (tex2Dlod(_DispTex02, float4(p.x / _PlaneSize*_SubTile02 + 0.5 + Suboffset02, p.z / _PlaneSize*_SubTile02 + 0.5 + Suboffset02, 0, 0)).r - 0.5) * _Displacement;

		float d = Md + Sd01 *_SubSt01 + Sd02 * _SubSt02;
		p.xyz += float3(0, 1, 0) * d;
		return p;
	}
#ifdef UNITY_CAN_COMPILE_TESSELLATION
	struct TessVertex {
		float4 vertex : INTERNALTESSPOS;
		fixed4 texcoord : TEXCOORD0;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
	};
	struct OutputPatchConstant {
		float edge[3]         : SV_TessFactor;
		float inside : SV_InsideTessFactor;
		float3 vTangent[4]    : TANGENT;
		float2 vUV[4]         : TEXCOORD;
		float3 vTanUCorner[4] : TANUCORNER;
		float3 vTanVCorner[4] : TANVCORNER;
		float4 vCWts          : TANWEIGHTS;
	};
	TessVertex tessvert(a2v v) {
		TessVertex o;
		o.vertex = v.vertex;
		o.normal = v.normal;
		o.tangent = v.tangent;
		o.texcoord = v.texcoord;
		return o;
	}
	float Tessellation(TessVertex v) {
		return _Tessellation;
	}
	float4 Tessellation(TessVertex v, TessVertex v1, TessVertex v2) {
		return UnityDistanceBasedTess(v.vertex, v1.vertex, v2.vertex, _MinDist, _MaxDist, _Tessellation);
	}
	OutputPatchConstant hullconst(InputPatch<TessVertex,3> v) {
		OutputPatchConstant o = (OutputPatchConstant)0;
		float4 ts = Tessellation(v[0], v[1], v[2]);
		o.edge[0] = ts.x;
		o.edge[1] = ts.y;
		o.edge[2] = ts.z;
		o.inside = ts.w;
		return o;
	}
	[domain("tri")]
	[partitioning("fractional_odd")]
	[outputtopology("triangle_cw")]
	[patchconstantfunc("hullconst")]
	[outputcontrolpoints(3)]
	TessVertex hull(InputPatch<TessVertex,3> v, uint id : SV_OutputControlPointID) {

		return v[id];
	}
	[domain("tri")]
	v2f domain(OutputPatchConstant tessFactors, const OutputPatch<TessVertex,3> vi, float3 bary : SV_DomainLocation) 
	{
		a2v v = (a2v)0;
		v.vertex = vi[0].vertex*bary.x + vi[1].vertex*bary.y + vi[2].vertex*bary.z;
		v.normal = vi[0].normal*bary.x + vi[1].normal*bary.y + vi[2].normal*bary.z;
		v.tangent = vi[0].tangent*bary.x + vi[1].tangent*bary.y + vi[2].tangent*bary.z;
		v.texcoord = vi[0].texcoord*bary.x + vi[1].texcoord*bary.y + vi[2].texcoord*bary.z;

		//float d = tex2Dlod(_DispTex, v.texcoord).r * _Displacement;//置换纹理采样
		//v.vertex.xyz += v.normal * d;//置换顶点

		//float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

		float4 binormal = float4(normalize(cross(v.normal, v.tangent)), 0);
		float4 NewPoint = GetVertNewPos(v.vertex);
		float4 TangentPoint = GetVertNewPos(v.vertex + v.tangent *_Smooth *0.05);
		float4 BinormalPoint = GetVertNewPos(v.vertex + binormal *_Smooth *0.05);
		float4 newTangent = TangentPoint - NewPoint;
		float4 newBinormal = BinormalPoint - NewPoint;

		v.vertex = NewPoint;
		float4 newNormal = float4(normalize(cross(newTangent, newBinormal)), 0);
		v.normal = newNormal;

		v2f o = vert(v);
		return o;
	}

	
#endif
	/*
	struct fragOut {
		float4 col: SV_Target;
		//float dep : SV_Depth;
	};//自定义的输出项 但是并没有用到depth的写入功能
	*/
	fixed4 frag(v2f i) : COLOR
	{
		//fragOut o;//申明
		fixed4 texColor = tex2D(_MainTex, i.uv);
		fixed3 norm = UnpackNormal(tex2D(_NormalMap, i.uv)) * _NormalAddFactor;
		float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
		fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
		fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
		
		half3 worldNormal = normalize(half3(dot(i.TtoW0.xyz, norm), dot(i.TtoW1.xyz, norm), dot(i.TtoW2.xyz, norm)));

		fixed3 worldView = fixed3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
		fixed3 worldRefl = reflect(-worldViewDir, worldNormal);

		//if (_IsBackFace == 1)
			//worldNormal = -worldNormal;

		//fixed4 grab = tex2Dproj(_GrabTexture, i.ScreenUV* float4(0.8, 0.8, 1, 1));//
		//UNITY_APPLY_FOG(i.fogCoord, reflecColor);

		fixed atten = LIGHT_ATTENUATION(i);
		//UNITY_OUTPUT_DEPTH(i.ScreenDepth);
		fixed3 ambi = UNITY_LIGHTMODEL_AMBIENT.xyz;

		
		
		float DistToCamera = distance(_WorldSpaceCameraPos, worldPos);
		_RefractionAmount = 1 - clamp(DistToCamera / _RefrMaxDistance + _RefractionAmount, 0, 1);//折射比例按照距离递减

		float3 VertWorldNormal = float3(i.TtoW0.z, i.TtoW1.z, i.TtoW2.z);
		float4 FinalTangentNormal =float4( VertWorldNormal.xz + norm.xy,1,1);

		fixed3 spec = _LightColor0.rgb * pow(saturate(dot(normalize(worldRefl), normalize(lightDir))), _Specular) * _Gloss;
		//******************************************菲涅尔/fresnel*************************************************
		float4 val = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);//对unity自动的反射球采样
		float3 CubeRefleColor = DecodeHDR(val, unity_SpecCube0_HDR);
		fixed fresnel = pow(1 - saturate(dot(worldViewDir, worldNormal)), 4);
		//******************************************反射/reflection************************************************
		float2 Reflecoffset = FinalTangentNormal.xy * _ReflecDistortion * _ReflecTexture_TexelSize.xy;
		fixed3 reflecColor = tex2D(_ReflecTexture,( i.ScreenPos.xy - Reflecoffset)/ i.ScreenPos.w);//得到反射颜色值
		//******************************************折射/refraction************************************************
		float2 Refracoffset = FinalTangentNormal.xy * _RefracDistortion * _RefractionTex_TexelSize.xy;//计算折射偏移值
		//i.ScreenPos.xy = i.ScreenPos.xy + Refracoffset;//并不是真正的折射，指示等量的扭曲
		float4 DistUV = i.ScreenPos + FinalTangentNormal * _RefracDistortion ;
		fixed3 refracCol = tex2D(_RefractionTex, (i.ScreenPos.xy + Refracoffset) / i.ScreenPos.w).rgb;//采样抓取的图像得到折射值
		fixed3 refracColN = tex2D(_RefractionTex, (i.ScreenPos.xy) / i.ScreenPos.w).rgb;//采样抓取的图像得到折射值
		fixed refrFix = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(DistUV)));
		if (LinearEyeDepth(refrFix) < (i.ScreenPos.z) / i.ScreenPos.w)
			refracCol = refracColN;
		//********************************************重建世界坐标************************************************
		float LinearDepth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, float4(
			(i.ScreenPos.xy + Refracoffset) / i.ScreenPos.w ,1,1
			))));//取得扭曲后的深度图
		float3 ReBuildworldPos = _WorldSpaceCameraPos + LinearDepth * i.interpolatedRay.xyz;
		//ReBuildworldPos /= 1000;
		//****************************************判断相机相对水面像素的位置******************************************
		float refraRatio = dot(normalize(worldNormal), (normalize(_WorldSpaceCameraPos - worldPos)));
		fixed4 fragColor;
		if (refraRatio > 0)//水面以上
		{
			float3 ViewDir = _WorldSpaceCameraPos - ReBuildworldPos;
			float3 SurfacePoint = ReBuildworldPos + ViewDir * abs(ReBuildworldPos.y / ViewDir.y);
			float FogDistance = length(SurfacePoint - ReBuildworldPos);
			float fogDensity = exp(-_FogDensity * abs(FogDistance));
			refracCol.rgb = lerp(_FogColor.rgb, refracCol.rgb, fogDensity);

			//fragColor.rgb = refracCol.rgb;
			fragColor.rgb = (reflecColor * fresnel + refracCol * (1 - fresnel) )/2 + spec;
		}
		
		else if (refraRatio < -_CriticalAngle)//水面下，但不完全反射
		{
			fragColor.rgb = reflecColor * 0.1 + refracCol;
			//fragColor.rgb = (DistToCamera / 100, 1, 1, 1);// lerp(fragColor.rgb, _FogColor, DistToCamera / 10);
		}
		else if (refraRatio > -_CriticalAngle)//水面下，但完全反射Critical angle
		{
			fragColor.rgb = reflecColor  * _Color;
			//fragColor = lerp(fragColor, _WaterFogColor, DistToCamera / 100);
		}
		/*
		else
		{
			float refraFactor = clamp(0, 1, -(refraRatio + 0.49) * 100);
			//fragColor.rgb = lerp(reflecColor * 0.1 + refracCol * 0.8, reflecColor* _Color, (1 - refraFactor));
			fragColor.rgb = lerp(fixed3(1,0,0), fixed3(0, 1, 0), (1 - refraFactor));
				//((reflecColor * 0.1 + refracCol * 0.8) *refraFactor + reflecColor * (1 - refraFactor));
		}
		
		//混合并添加高光
		fixed fresnel = _FresnelScale + (1 - _FresnelScale) * pow( 1 - dot(worldViewDir, worldNormal), 5);
		
		fixed3 diff = (_LightColor0.rgb * _Color)* saturate(dot(normalize(worldNormal),  normalize(lightDir))) ;
		//fixed3 diff = (_LightColor0.rgb * _Color + ReflectColor * _ReflColor.a* (1 - _RefractionAmount) + fixed4(refrCol, 1) *_RefractionAmount)* saturate(dot(normalize(worldNormal), normalize(lightDir)));

		fixed3 lightRefl = reflect(-lightDir, worldNormal);
		fixed3 spec = _LightColor0.rgb * pow(saturate(dot(normalize(lightRefl), normalize(worldViewDir))), _Specular) * _Gloss;

		fixed4 fragColor;
		//fragColor.rgb = float3((ambi + (diff + spec) * atten) * texColor);
		fragColor.rgb = float3(ambi + lerp(diff, ReflectColor * _ReflColor.a, saturate(fresnel)) * atten) + refrCol *_RefractionAmount;
		*/

		UNITY_APPLY_FOG(i.fogCoord, fragColor);
		fragColor.a = _TransParent;
		//UNITY_OUTPUT_DEPTH(i.ScreenDepth);

		//o.col = fragColor;

		//return float4(ReBuildworldPos, 1);
		return fragColor;
		
	}

		ENDCG
	}
	}
		FallBack "Diffuse"
}