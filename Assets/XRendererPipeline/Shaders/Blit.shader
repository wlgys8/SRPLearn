Shader "Hidden/SRPLearn/Blit"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque"}
        LOD 100

        HLSLINCLUDE
        #pragma enable_cbuffer
        #include "../ShaderLibrary/SpaceTransform.hlsl"
        ENDHLSL

        Pass
        {
            Name "Blit"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            

            #pragma vertex Vertex
            #pragma fragment Fragment


            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                half4 positionCS    : SV_POSITION;
                half2 uv            : TEXCOORD0;
            };

            Texture2D _BlitTex;
 
            SamplerState sampler_BlitTex;


            Varyings Vertex(Attributes input)
            {
                Varyings output;
                output.positionCS = ObjectToHClipPosition(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                half4 col = _BlitTex.Sample(sampler_BlitTex, input.uv);
                return col;
            }
            ENDHLSL
        }
    }
}
