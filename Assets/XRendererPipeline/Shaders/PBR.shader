Shader "SRPLearn/PBR"
{
    

    Properties
    {
        _AlbedoMap ("Texture", 2D) = "white" {}
        _MetalMap ("MetalMap", 2D) = "white" {}
        _BumpMap("BumpMap",2D) = "white" {}
        _IBLSpec("IBL Specular",Cube) = "black" {}
        _Metalness("Metalness",Range(0.01,1)) = 0.5
        _Roughness("Roughness",Range(0.01,0.99)) = 0.5
        _Color("Color",Color) = (1,1,1,1)
        [HideInInspector]_IBLSpecMaxMip("IBLSpecMaxMip",Int) = 1
        [Toggle(_RECEIVE_SHADOWS_OFF)] _RECEIVE_SHADOWS_OFF ("Receive Shadows Off?", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100

        HLSLINCLUDE
        #pragma enable_cbuffer
        #include "./PBR.hlsl"

        ENDHLSL

        Pass
        {
            Name "DEFAULT"
            Tags {"LightMode"="XForwardBase"}

            Cull Back

            HLSLPROGRAM

            #pragma multi_compile _ X_SHADOW_BIAS_RECEIVER_PIXEL
            #pragma multi_compile _ X_SHADOW_PCF
            #pragma shader_feature X_CSM_BLEND

            #pragma shader_feature _RECEIVE_SHADOWS_OFF
            #pragma shader_feature _PBR_IBL_SPEC
            #pragma shader_feature _PBR_IBL_DIFF
            #pragma shader_feature ENABLE_NORMAL_MAP

            #pragma vertex VertForward
            #pragma fragment FragForward

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM

            #pragma multi_compile _ X_SHADOW_BIAS_CASTER_VERTEX

            #include "../ShaderLibrary/ShadowCaster.hlsl"

            #pragma vertex ShadowCasterVertex
            #pragma fragment ShadowCasterFragment
        
            ENDHLSL
        }

        Pass
        {
            Name "ShadowDebug"
            Tags{"LightMode" = "ShadowDebug"}

            // ZWrite Off
            // ZTest LEqual
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha


            HLSLPROGRAM
            #pragma shader_feature X_CSM_BLEND
            #include "../ShaderLibrary/ShadowDebug.hlsl"

            #pragma vertex ShadowDebugVertex
            #pragma fragment ShadowDebugFragment
        
            ENDHLSL
        }

        Pass{
            Tags {"LightMode"="Deferred"}
            
            Name "DEFERRED"

            Cull Back
            
            HLSLPROGRAM

            #pragma shader_feature GBUFFER_ACCURATE_NORMAL
        
            #pragma vertex VertGBuffer
            #pragma fragment FragGBuffer

            ENDHLSL
        }
    }

    CustomEditor "SRPLearn.Editor.PBRShaderGUI"
}
