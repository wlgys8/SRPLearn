#ifndef X_BLINN_PHONG_FRAG_INCLUDED
#define X_BLINN_PHONG_FRAG_INCLUDED

#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/Shadow.hlsl"
#include "../ShaderLibrary/SpaceTransform.hlsl"

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
fixed4 _SpecularColor;
fixed4 _Color;
float4 _MainTex_ST;
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


half4 BlinnPhongFinal(Varyings input){
    half4 diffuseColor = UNITY_SAMPLE_TEX2D(_MainTex,input.uv);
    float3 positionWS = input.positionWS;
    float3 normalWS = normalize(input.normalWS);

    //定义几何数据
    BlinnPhongGemo gemo;
    gemo.normal = normalWS;
    gemo.viewDir = normalize(_WorldSpaceCameraPos - positionWS);
    gemo.position = positionWS;

    //定义渲染参数
    BlinnPhongProperty property;
    property.diffuseColor = diffuseColor * _Color;
    property.specularColor = _SpecularColor;
    property.shininess = _Shininess;

    //计算主光源(平行光)BlinnPhong光照
    XDirLight mainLight = GetMainLight();
    half4 mainLightColor = BlinnPhong(mainLight.direction,gemo,property);
    mainLightColor *= mainLight.color;

    //计算主光源(平行光)阴影
    float shadowAtten = GetMainLightShadowAtten(positionWS,normalWS);
    mainLightColor *= shadowAtten;

    //计算点光源的BlinnPhong光照
    half4 pointLightsColor = BlinnPhongPointLights(gemo,property);

    half4 color = _XAmbientColor + mainLightColor + pointLightsColor;
    color.a = 1;
    return color;
}


half4 PassFragment(Varyings input) : SV_Target
{
    return BlinnPhongFinal(input);
}

half4 PassFragmentTransparent(Varyings input):SV_Target
{
    half4 color = BlinnPhongFinal(input);
    color.a = _Color.a;
    return color;
}

#endif
