Shader "SRPLearn/BlinnPhongSpecular"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Shininess("Shininess",Range(10,128)) = 50
        _SpecularColor("SpecularColor",Color) = (1,1,1,1)
        _Color("Color",Color) = (1,1,1,1)
        [Toggle(_RECEIVE_SHADOWS_OFF)] _RECEIVE_SHADOWS_OFF ("Receive Shadows Off?", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="XForwardBase"}
        LOD 100

        HLSLINCLUDE
        #pragma enable_cbuffer
        #include "./BlinnPhongFrag.hlsl"


        ENDHLSL

        Pass
        {
            Name "DEFAULT"

            Cull Back

            HLSLPROGRAM
            
            #pragma vertex PassVertex
            #pragma fragment PassFragment

            #pragma shader_feature _RECEIVE_SHADOWS_OFF
      
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

            #pragma vertex ShadowCasterVertex
            #pragma fragment ShadowCasterFragment
        
            ENDHLSL
        }
    }
}
