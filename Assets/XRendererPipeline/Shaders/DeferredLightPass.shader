Shader "Hidden/SRPLearn/DeferredLightPass"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque"}
        LOD 100

        HLSLINCLUDE
        #pragma enable_cbuffer
        #include "../ShaderLibrary/DeferredParams.hlsl"
        #include "../ShaderLibrary/SpaceTransform.hlsl"
        #include "../ShaderLibrary/PBRDeferred.hlsl"
        #include "../ShaderLibrary/TileDeferredInput.hlsl"
        
        ENDHLSL

        Pass
        {
            Name "DEFAULT"
            ZTest Off
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma multi_compile _ X_SHADOW_BIAS_RECEIVER_PIXEL
            #pragma multi_compile _ X_SHADOW_PCF
            #pragma shader_feature X_CSM_BLEND
            #pragma shader_feature _RECEIVE_SHADOWS_OFF
            #pragma shader_feature DEFERRED_BUFFER_DEBUGON

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


            SamplerState sampler_pointer_clamp;
            Texture2D _XDepthTexture;
 
            #if DEFERRED_BUFFER_DEBUGON
            half3 ShadeDebug(PBRShadeInput shadeInput,float depth,uint lightCount){
                int debugMode = _DeferredDebugMode;
                half3 color = 0;
                if(debugMode == 1){ //albedo
                    color = shadeInput.albedo;
                }else if(debugMode == 2){ //normal
                    color = (shadeInput.normal + 1) * 0.5;
                }else if(debugMode == 3){ //position
                    color = shadeInput.positionWS / 20;
                }else if(debugMode == 4){//metalness
                    color = shadeInput.metalness;
                }else if(debugMode == 5){ //roughness   
                    color = 1 - shadeInput.smooth;
                }else if(debugMode == 6){ //visible light count
                    color = lightCount * 1.0 / MAX_LIGHT_COUNT_PER_TILE;
                }else if(debugMode == 7){ //depth
                    color = depth;
                }
                return color;
            }
            #endif

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                output.positionCS = ObjectToHClipPosition(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {   
                float2 uv = input.uv;
                float depth = _XDepthTexture.Sample(sampler_pointer_clamp,input.uv).x;
                float2 coord = _ScreenParams.xy * uv; 
                uint2 tileId = floor(coord / _DeferredTileParams.xy);
                uint tileIndex = tileId.y * _DeferredTileParams.z + tileId.x;
                uint lightCount = _TileLightsArgsBuffer[tileIndex];

                half4 g0 =  _GBuffer0.Sample(sampler_pointer_clamp,input.uv);
                half4 g1 =  _GBuffer1.Sample(sampler_pointer_clamp,input.uv);
                half4 g2 =  _GBuffer2.Sample(sampler_pointer_clamp,input.uv);
                half4 g3 =  _GBuffer3.Sample(sampler_pointer_clamp,input.uv);
                PBRShadeInput shadeInput;
                float3 positionWS = ReconstructPositionWS(uv,depth);
                shadeInput.positionWS = positionWS;
                DecodeGBuffer(shadeInput,g0,g1,g2,g3);

                half3 color = 0;

                #if DEFERRED_BUFFER_DEBUGON
                    color = ShadeDebug(shadeInput,depth,lightCount);
                #else
                    //着色点几何信息
                    DECLARE_SHADE_POINT_DESC(sPointDesc,shadeInput);
                    //pbr材质相关
                    DECLARE_PBR_DESC(pbrDesc,shadeInput);
                    //平行光
                    ShadeLightDesc mainLightDesc = GetMainLightShadeDescWithShadow(shadeInput.positionWS,shadeInput.normal);
                    color = PBRShading(pbrDesc,sPointDesc,mainLightDesc);

                    uint tileLightOffset = tileIndex * MAX_LIGHT_COUNT_PER_TILE;
                    for(uint i = 0; i < lightCount; i ++){
                        uint lightIndex = _TileLightsIndicesBuffer[tileLightOffset + i];
                        float4 lightSphere = _DeferredOtherLightPositionAndRanges[lightIndex];
                        half4 lightColor = _DeferredOtherLightColors[lightIndex];
                        ShadeLightDesc lightDesc = GetPointLightShadeDesc(lightSphere,lightColor,shadeInput.positionWS);
                        color += PBRShading(pbrDesc,sPointDesc,lightDesc);
                    }
                #endif

                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
