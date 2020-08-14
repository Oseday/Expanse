// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/LasersIndirect"
{
    Properties
    {
        //textur ("Texture", 2D) = "white" {}
        //startTime ("startTime", float) = 0
        //random ("random", float) = 0
        //width ("width", float) = 0.05
        //start ("start", Vector) = (0.0,0.0,0.0,0.0)
        //end ("end", Vector) = (0.0,0.0,0.0,0.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {






HLSLPROGRAM
#pragma target 5.0
#pragma vertex vertex
#pragma fragment fragment
#pragma multi_compile_instancing

            //#include "Lasers.hlsl"
#include "UnityCG.cginc"

struct VertexInput {
    float4 vertex : POSITION;
    uint vertexidsv : SV_VertexID;
    uint instanceid : SV_InstanceID;
    //UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexData {
    float4 vertex : SV_POSITION;
    float4 color : COLOR0;
    uint instanceid : SV_InstanceID;
   // UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct MeshProperties {
    float3 start;
    float3 end;
    float width;
    float random;
    float startTime;
};

RWStructuredBuffer<MeshProperties> Lasers;

uint mod(uint x, uint y)
{
  return x - y * floor(x/y);
}

VertexData vertex (VertexInput v)
{
    VertexData o;
    o.instanceid = v.instanceid;

    //UNITY_SETUP_INSTANCE_ID(v);
    //UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.

    float3 _start = Lasers[o.instanceid].start;//UNITY_ACCESS_INSTANCED_PROP(Props, start);
    float3 _end = Lasers[o.instanceid].end;//UNITY_ACCESS_INSTANCED_PROP(Props, end);
    float _width = Lasers[o.instanceid].width;//UNITY_ACCESS_INSTANCED_PROP(Props, width);
    float3 vel = normalize(_end-_start);

    float3 lookdir;
    float4 nd;

    uint vid = mod(v.vertexidsv,4);

    [branch] switch(vid){
        case 0: 
            o.color = float4(0,1,0,0);
            lookdir = _WorldSpaceCameraPos-_end;
            nd = float4(_end.xyz-normalize(cross(lookdir,vel))*_width, 1.0);
            o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject,nd));
            break;
        case 1: 
            o.color = float4(1,1,0,0);
            lookdir = _WorldSpaceCameraPos-_end;
            nd = float4(_end.xyz+normalize(cross(lookdir,vel))*_width, 1.0);
            o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject,nd));
            break;
        case 2: 
            o.color = float4(0,0,0,0);
            lookdir = _WorldSpaceCameraPos-_start;
            nd = float4(_start.xyz-normalize(cross(lookdir,vel))*_width, 1.0);
            o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject,nd));
            break;
        case 3:
            o.color = float4(1,0,0,0);
            lookdir = _WorldSpaceCameraPos-_start;
            nd = float4(_start.xyz+normalize(cross(lookdir,vel))*_width, 1.0);
            o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject,nd));
            break;
        default:
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.color = float4(1,0,0,0);
            break;
    }

    return o;
}

float4 fragment (VertexData i) : SV_Target
{
    //UNITY_SETUP_INSTANCE_ID(i);
    return i.color;//UNITY_ACCESS_INSTANCED_PROP(Props, end);//float4(end.xyz,0.5);//float4(0.2,0.3,1,0.5);
}











ENDHLSL}}}
