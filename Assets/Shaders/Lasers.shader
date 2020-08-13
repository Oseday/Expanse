Shader "Unlit/Lasers"
{
    Properties
    {
        textur ("Texture", 2D) = "white" {}
        startTime ("startTime", float) = 0
        random ("random", float) = 0
        width ("width", float) = 0.05
        start ("start", Vector) = (0.0,0.0,0.0,0.0)
        end ("end", Vector) = (0.0,0.0,0.0,0.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment
            #pragma multi_compile_instancing

            #include "Lasers.hlsl"

            ENDHLSL
        }
    }
}
