// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Lasers/Indirect"
{
	Properties
	{
		_Color ("Color", Color) = (1.0,0.0,0.0,0.0)
		_Sharpness ("Sharpness", Range(1,100)) = 45
	}
	SubShader
	{
		Tags { "Queue"="Transparent" }
		LOD 100

		Pass
		{
			//Blend [_SrcBlend] [_DstBlend]
			//ZWrite [_ZWrite]

			Blend SrcAlpha One 
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
float _Sharpness;

float4 fragment (VertexData i) : SV_Target
{
	float4 colb;

	float depth = tex2D(_CameraDepthTexture, i.projPos.xy/i.projPos.w);
	float depthdiff = 1-clamp((1.0/depth-1.0/i.vertex.z),0,1)*1.0;

	
	float tb = pow(-4.0*(i.color.w*i.color.w)+1.0,_Sharpness);

	float alpha = clamp(tb-pow(depthdiff,4.0),0,1); //smoothing

	colb = float4(tb*(1-_Color.x)+_Color.x, tb*(1-_Color.y)+_Color.y, tb*(1-_Color.z)+_Color.z, _Color.w*alpha);

	return colb;
}











ENDHLSL}}}
