Shader "Hidden/SRPLearn/Blit"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque"}
        LOD 100

        HLSLINCLUDE
        #pragma enable_cbuffer
        
        #include "../ShaderLibrary/SpaceTransform.hlsl"
        #include "../ShaderLibrary/FXAA.hlsl"
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
            #pragma shader_feature __ FXAA_V1 FXAA_QUALITY FXAA_CONSOLE
            #pragma shader_feature __ FXAA_DEBUG_EDGE
            #pragma shader_feature __ FXAA_DEBUG_CULL_PASS


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
                
                #if FXAA_ENABLE
                return FXAA(_BlitTex,input.uv);
                #else
                float2 coord = _ScreenParams.xy * input.uv;

                return _BlitTex.SampleLevel(sampler_BlitTex,input.uv,0);
                // return _BlitTex.Sample(sampler_BlitTex, input.uv);
                #endif
                
            }
            ENDHLSL
        }
    }
}
