#include "UnityCG.cginc"

struct VertexInput {
    float4 vertex : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    uint id : SV_VertexID;
};

struct VertexData {
    float4 vertex : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float4 color : COLOR0;
};

UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(float4, start)
    UNITY_DEFINE_INSTANCED_PROP(float4, end)
    UNITY_DEFINE_INSTANCED_PROP(float, width)
    UNITY_DEFINE_INSTANCED_PROP(float, random)
    UNITY_DEFINE_INSTANCED_PROP(float, startTime)
UNITY_INSTANCING_BUFFER_END(Props)

VertexData vertex (VertexInput v)
{
    VertexData o;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.

    start = UNITY_ACCESS_INSTANCED_PROP(Props, start);
    end = UNITY_ACCESS_INSTANCED_PROP(Props, end);
    width = UNITY_ACCESS_INSTANCED_PROP(Props, width);
    float4 vel = normalize(end-start);

    float3 lookdir;
    float4 nd;

    [branch] switch(v.id){
        case 0: 
            o.color = float4(0,1,0,0);//
            lookdir = _WorldSpaceCameraPos-end;
            nd = float4(end.xyz-normalize(cross(lookdir,vel))*width, 1.0);
            o.vertex = mul(UNITY_MATRIX_MVP, mul(unity_WorldToObject,nd));
            break;
        case 1: 
            o.color = float4(1,1,0,0);
            lookdir = _WorldSpaceCameraPos-end;
            nd = float4(end.xyz+normalize(cross(lookdir,vel))*width, 1.0);
            o.vertex = mul(UNITY_MATRIX_MVP, mul(unity_WorldToObject,nd));
            break;
        case 2: 
            o.color = float4(0,0,0,0);
            lookdir = _WorldSpaceCameraPos-start;
            nd = float4(start.xyz-normalize(cross(lookdir,vel))*width, 1.0);
            o.vertex = mul(UNITY_MATRIX_MVP, mul(unity_WorldToObject,nd));
            break;
        case 3:
            o.color = float4(1,0,0,0);
            lookdir = _WorldSpaceCameraPos-start;
            nd = float4(start.xyz+normalize(cross(lookdir,vel))*width, 1.0);
            o.vertex = mul(UNITY_MATRIX_MVP, mul(unity_WorldToObject,nd));
            break;
    }

    return o;
}

float4 fragment (VertexData i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);
    return i.color.xyww;//UNITY_ACCESS_INSTANCED_PROP(Props, end);//float4(end.xyz,0.5);//float4(0.2,0.3,1,0.5);
}
