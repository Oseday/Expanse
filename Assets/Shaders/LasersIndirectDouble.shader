// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Lasers/IndirectDouble"
{
	Properties
	{
		//textur ("Texture", 2D) = "white" {}
		//startTime ("startTime", float) = 0
		//random ("random", float) = 0
		//width ("width", float) = 0.05
		//start ("start", Vector) = (0.0,0.0,0.0,0.0)
		//end ("end", Vector) = (0.0,0.0,0.0,0.0)		
		//[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend", Float) = 6
		//[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination Blend", Float) = 2
		//[HideInInspector][Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
		//[HideInInspector][Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 0
		_Color ("Color", Color) = (1.0,0.0,0.0,0.0)
		_ColorB ("Color", Color) = (1.0,0.0,0.0,0.0)
		_Sharpness ("Sharpness", Range(1,100)) = 45
		_Speed ("Speed", Range(0,100)) = 10
		_Frequency ("Frequency", Range(0,100)) = 10
	}
	SubShader
	{
		Tags { "Queue"="Transparent" }
		LOD 100

		Pass
		{
			//Blend [_SrcBlend] [_DstBlend]
			//ZWrite [_ZWrite]

			Blend SrcAlpha OneMinusSrcAlpha 
			ZWrite Off





HLSLPROGRAM
#pragma target 4.5
#pragma vertex vertex
#pragma fragment fragment
#pragma multi_compile_instancing

			//#include "Lasers.hlsl"
#include "UnityCG.cginc"

struct VertexInput {
	float4 vertex : POSITION;
	uint vertexidsv : SV_VertexID;
	uint instanceid : SV_InstanceID;
	//float depth : SV_Depth;
	//UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexData {
	float4 vertex : SV_Position;
	float4 color : COLOR0;
	uint instanceid : SV_InstanceID;
	float4 projPos : TEXCOORD0;
	float4 depth : DEPTH;
	//float depth : SV_Depth;
   // UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Laser {
	float3 start;
	float3 end;
	float width;
	float random;
	float startTime;
};

StructuredBuffer<Laser> _Lasers;

sampler2D _CameraDepthTexture;
//UNITY_DECLARE_TEX2DARRAY(_CameraDepthTexture);
//Texture2D _CameraDepthTexture;
//SamplerState _CameraDepthTexture;


uint mod(uint x, uint y)
{
  return x - y * floor(x/y);
}

float mod(float x, float y)
{
  return x - y * floor(x/y);
}

VertexData vertex (VertexInput v)
{
	VertexData o;
	o.instanceid = v.instanceid;
	//o.depth = v.depth;

	//UNITY_SETUP_INSTANCE_ID(v);
	//UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.

	float3 _start = _Lasers[o.instanceid].start;//UNITY_ACCESS_INSTANCED_PROP(Props, start);
	float3 _end = _Lasers[o.instanceid].end;//UNITY_ACCESS_INSTANCED_PROP(Props, end);
	float _width = _Lasers[o.instanceid].width;//UNITY_ACCESS_INSTANCED_PROP(Props, width);
	float3 rel = _end-_start;
	float3 vel = normalize(rel);

	float3 lookdir;
	float4 nd;

	uint vid = mod(v.vertexidsv,4);

	[branch] switch(vid){
		case 0: 
			o.color = float4(0,1,length(rel),0.5);
			lookdir = _WorldSpaceCameraPos-_end;
			nd = float4(_end.xyz-normalize(cross(lookdir,vel))*_width, 1.0);
			o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject,nd));
			break;
		case 1: 
			o.color = float4(1,1,length(rel),-0.5);
			lookdir = _WorldSpaceCameraPos-_end;
			nd = float4(_end.xyz+normalize(cross(lookdir,vel))*_width, 1.0);
			o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject,nd));
			break;
		case 2: 
			o.color = float4(0,0,0,0.5);
			lookdir = _WorldSpaceCameraPos-_start;
			nd = float4(_start.xyz-normalize(cross(lookdir,vel))*_width, 1.0);
			o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject,nd));
			break;
		case 3:
			o.color = float4(1,0,0,-0.5);
			lookdir = _WorldSpaceCameraPos-_start;
			nd = float4(_start.xyz+normalize(cross(lookdir,vel))*_width, 1.0);
			o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject,nd));
			break;
		default:
			o.vertex = UnityObjectToClipPos(v.vertex + float4(0,o.instanceid*1.1,0,0));
			o.color = float4(vid/4.0, _Lasers[o.instanceid].random, .5, 0);
			break;
	}

	o.projPos = ComputeScreenPos(o.vertex);
	o.depth = -mul(UNITY_MATRIX_MV, v.vertex).z * _ProjectionParams.w;

	return o;
}


float4 _Color;
float4 _ColorB;
float _Sharpness;
float _Speed;
float _Frequency;

float4 fragment (VertexData i) : SV_Target
{
	float4 cola;
	float4 colb;

	float depth = tex2D(_CameraDepthTexture, i.projPos.xy/i.projPos.w);
	float depthdiff = 1-clamp((1.0/depth-1.0/i.vertex.z),0,1)*1.0;

	float tick = _Time.w*_Speed;
	
	//float t = pow(-4.0*(i.color.w*i.color.w)+1.0,_Sharpness);

	float ta = 0.25*cos(_Frequency*i.color.z + tick)+0.5 - _Sharpness*pow(i.color.x - 0.25*sin(_Frequency*i.color.z + tick)-0.5, 2);
	ta = clamp(ta,0,1);
	
	float tb = 0.25*cos(_Frequency*i.color.z+3.145928 + tick)+0.5 - _Sharpness*pow(i.color.x - 0.25*sin(_Frequency*i.color.z+3.145928 + tick)-0.5, 2);
	tb = clamp(tb,0,1);

	float alphaa = clamp(ta-pow(depthdiff,4.0),0,1); //smoothing

	//cola = float4(ta*(1-_Color.x)+_Color.x, ta*(1-_Color.y)+_Color.y, ta*(1-_Color.z)+_Color.z, _Color.w*alphaa);
	cola = float4(ta*_Color.x, ta*_Color.y, ta*_Color.z, ta*_Color.w*alphaa);
	
	float alphab = clamp(tb-pow(depthdiff,4.0),0,1); //smoothing

	//colb = float4(tb*(1-_ColorB.x)+_ColorB.x, tb*(1-_ColorB.y)+_ColorB.y, tb*(1-_ColorB.z)+_ColorB.z, _ColorB.w*alphab);
	colb = float4(tb*_ColorB.x, tb*_ColorB.y, tb*_ColorB.z, tb*_ColorB.w*alphab);


	//col = float4(1,t,t,alpha);
	return cola*2 + 2*colb;
}











ENDHLSL}}}
