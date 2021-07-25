#ifndef X_PBR_FRAG_INCLUDED
#define X_PBR_FRAG_INCLUDED

#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/Shadow.hlsl"
#include "../ShaderLibrary/SpaceTransform.hlsl"
#include "../ShaderLibrary/PBR_GGX.hlsl"

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

UNITY_DECLARE_TEX2D(_AlbedoMap);
UNITY_DECLARE_TEX2D(_MetalMap);
UNITY_DECLARE_TEX2D(_BRDFLUT);

samplerCUBE _IBLSpec;


CBUFFER_START(UnityPerMaterial)
float _Metalness;
float _Roughness;
half4 _Color;
float4 _AlbedoMap_ST;
uint _IBLSpecMaxMip;
CBUFFER_END


Varyings PassVertex(Attributes input)
{
    Varyings output;
    output.positionCS = UnityObjectToClipPos(input.positionOS);
    output.uv = TRANSFORM_TEX(input.uv, _AlbedoMap);
    output.normalWS = mul(unity_ObjectToWorld, float4( input.normalOS, 0.0 )).xyz;
    output.positionWS = mul(unity_ObjectToWorld,input.positionOS).xyz;
    return output;
}

half4 PBRFrag(Varyings input){
    half4 albedo = UNITY_SAMPLE_TEX2D(_AlbedoMap,input.uv);
    half4 metalInfo = UNITY_SAMPLE_TEX2D(_MetalMap,input.uv);
    float3 positionWS = input.positionWS;
    float3 normalWS = normalize(input.normalWS);
    float3 viewDir = normalize(_WorldSpaceCameraPos - positionWS);

    XDirLight mainLight = GetMainLight();

    //初始化相关参数
    float3 lightDir = mainLight.direction;

    PBRDesc pbrDesc;
    pbrDesc.albedo = albedo * _Color;
    pbrDesc.roughness = (1 - metalInfo.a * (1 - _Roughness)); //MetalMap的alpha通道保存的是光滑度
    pbrDesc.metalness = _Metalness * metalInfo.r; //MetalMap的r通道保存了金属度

    BRDFData brdfData;
    InitializeData(lightDir,viewDir,normalWS,pbrDesc,brdfData);

    //直接光的辐照度
    float shadowAtten = GetMainLightShadowAtten(positionWS,normalWS);
    half3 directIrradiance = brdfData.NoL * mainLight.color * shadowAtten;
    half3 color = BRDF(pbrDesc,brdfData) * directIrradiance;

    half3 indirectColor = 0;
    
    #if _PBR_IBL_SPEC
    //IBL Specular
    float3 reflectDir = reflect(-viewDir,normalWS);
    float3 prefilteredColor = texCUBElod(_IBLSpec,float4(reflectDir,(pbrDesc.roughness)*_IBLSpecMaxMip)).rgb;
    float4 scaleBias = UNITY_SAMPLE_TEX2D(_BRDFLUT,float2(brdfData.NoV,pbrDesc.roughness));
    half3 indirectSpec = BRDFIBLSpec(pbrDesc,brdfData,scaleBias.xy) * prefilteredColor;
    indirectColor += indirectSpec;
    #endif

    #if _PBR_IBL_DIFF
    //IBL Diffuse 
    float3 indirectDiff = texCUBElod(_IBLSpec,float4(normalWS,_IBLSpecMaxMip / 3 )) * pbrDesc.albedo * INV_PI;
    // half3 ks = F_SchlickRoughness(pbrDesc.f0,brdfData.NoV,pbrDesc.roughness);
    half3 ks = F_Schlick(pbrDesc.f0,brdfData.NoV);
    half3 kd = (1 - ks)*(1 - pbrDesc.metalness);

    indirectColor += kd * indirectDiff;
    #endif

    color += indirectColor;

    return half4(color,1); 
}


half4 PassFragment(Varyings input) : SV_Target
{
    return PBRFrag(input);
}


#endif
