Shader "SRPLearn/BlinnPhongSpecularTransparent"
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
        Tags { "RenderType"="Transparent" "LightMode"="XForwardBase" "Queue" = "Transparent"}
        LOD 100

        ZWrite Off

        HLSLINCLUDE
        #pragma enable_cbuffer

        #include "./BlinnPhongFrag.hlsl"

        ENDHLSL

        Pass
        {
            Name "DEFAULT"
            
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            
            #pragma vertex PassVertex
            #pragma fragment PassFragmentTransparent
            #pragma shader_feature _RECEIVE_SHADOWS_OFF

            ENDHLSL
        }
    
    }
}
