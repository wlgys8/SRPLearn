Shader "SRPLearn/BlinnPongSpecular"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Shininess("Shininess",Range(10,128)) = 50
        _SpecularColor("SpecularColor",Color) = (1,1,1,1)
        [Toggle(_RECEIVE_SHADOWS_OFF)] _RECEIVE_SHADOWS_OFF ("Receive Shadows Off?", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="XForwardBase"}
        LOD 100

        HLSLINCLUDE
        #pragma enable_cbuffer
        #include "UnityCG.cginc"
        #include "../ShaderLibrary/Light.hlsl"
        #include "../ShaderLibrary/Shadow.hlsl"



        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        CBUFFER_END
        

        ENDHLSL

        Pass
        {
            Name "DEFAULT"
            
            HLSLPROGRAM
            
            #pragma vertex PassVertex
            #pragma fragment PassFragment

            #pragma shader_feature _RECEIVE_SHADOWS_OFF

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS   : TEXCOORD2;
            };
            UNITY_DECLARE_TEX2D(_MainTex);
            CBUFFER_START(UnityPerMaterial)
            float _Shininess;
            CBUFFER_END

            Varyings PassVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = UnityObjectToClipPos(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = mul(unity_ObjectToWorld, float4( input.normalOS, 0.0 )).xyz;
                output.positionWS = mul(unity_ObjectToWorld,input.positionOS).xyz;
                return output;
            }

            half4 PassFragment(Varyings input) : SV_Target
            {

                half4 diffuseColor = UNITY_SAMPLE_TEX2D(_MainTex,input.uv);
                float3 positionWS = input.positionWS;
                float3 normalWS = input.normalWS;
                half4 color = BlinnPongLight(positionWS,normalWS,_Shininess,diffuseColor,half4(1,1,1,1));
                return (1 - GetMainLightShadowAtten(positionWS,normalWS)) * color;
            }
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
