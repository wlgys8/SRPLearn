#ifndef X_PBR_FRAG_INCLUDED
#define X_PBR_FRAG_INCLUDED

#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/Shadow.hlsl"
#include "../ShaderLibrary/SpaceTransform.hlsl"
#include "../ShaderLibrary/PBR_GGX.hlsl"
#include "../ShaderLibrary/PBRDeferred.hlsl"
#include "./PBRInput.hlsl"


Varyings VertForward(Attributes input)
{
    Varyings output;
    output.positionCS = UnityObjectToClipPos(input.positionOS);
    output.uv = TRANSFORM_TEX(input.uv, _AlbedoMap);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.positionWS = mul(unity_ObjectToWorld,input.positionOS).xyz;
    #if ENABLE_NORMAL_MAP
    output.tangentWS = TransformObjectToWorldVector(input.tangentOS);
    #endif
    return output;
}


half4 PBRFrag(Varyings input){
    half4 albedo = UNITY_SAMPLE_TEX2D(_AlbedoMap,input.uv);
    half4 metalInfo = UNITY_SAMPLE_TEX2D(_MetalMap,input.uv);
    float3 positionWS = input.positionWS;
    float3 normalWS = normalize(input.normalWS);
    float3 viewDir = normalize(_WorldSpaceCameraPos - positionWS);
    half NoV = max(0,dot(normalWS,viewDir));

    #if ENABLE_NORMAL_MAP
    float3 tangentWS = normalize(input.tangentWS);
    float3 binormalWS = cross(normalWS,tangentWS);
    float3x3 TBN = float3x3(tangentWS,binormalWS,normalWS);
    half4 normalSampled = UNITY_SAMPLE_TEX2D(_BumpMap,input.uv);
    normalSampled = DecodeNormalFromTex(normalSampled);
    normalWS = mul(normalSampled,TBN);
    #endif

    ShadePointDesc sPointDesc;
    sPointDesc.positionWS = positionWS;
    sPointDesc.normalWS = normalWS;

    PBRDesc pbrDesc = InitPBRDesc((1 - metalInfo.a * (1 - _Roughness)),_Metalness * metalInfo.r,albedo * _Color);

    ShadeLightDesc mainLightDesc = GetMainLightShadeDescWithShadow(positionWS,normalWS);

    //直接光的PBR着色
    half3 color = PBRShading(pbrDesc,sPointDesc,mainLightDesc);
    
    //点光源的辐照度计算
    int lightCount = GetOtherLightCount();
    for(int i = 0; i < lightCount; i ++){
        ShadeLightDesc lightDesc = GetOtherLightShadeDesc(i,positionWS);
        color += PBRShading(pbrDesc,sPointDesc,lightDesc);
    }

    half3 indirectColor = 0;
    
    #if _PBR_IBL_SPEC
    //IBL Specular
    float3 reflectDir = reflect(-viewDir,normalWS);
    float3 prefilteredColor = texCUBElod(_IBLSpec,float4(reflectDir,(pbrDesc.roughness)*_IBLSpecMaxMip)).rgb;
    float4 scaleBias = UNITY_SAMPLE_TEX2D(_BRDFLUT,float2(NoV,pbrDesc.roughness));
    half3 indirectSpec = BRDFIBLSpec(pbrDesc,scaleBias.xy) * prefilteredColor;
    indirectColor += indirectSpec;
    #endif

    #if _PBR_IBL_DIFF
    //IBL Diffuse 
    float3 indirectDiff = texCUBElod(_IBLSpec,float4(normalWS,_IBLSpecMaxMip / 3 )) * pbrDesc.albedo * INV_PI;
    // half3 ks = F_SchlickRoughness(pbrDesc.f0,NoV,pbrDesc.roughness);
    half3 ks = F_Schlick(pbrDesc.f0,NoV);
    half3 kd = (1 - ks)*(1 - pbrDesc.metalness);

    indirectColor += kd * indirectDiff;
    #endif

    color += indirectColor;

    return half4(color,1); 
}


half4 FragForward(Varyings input) : SV_Target
{
    return PBRFrag(input);
}

//*******************GBuffer Begin*****************//

GBufferVaryings VertGBuffer(Attributes input)
{
    GBufferVaryings output;
    output.positionCS = UnityObjectToClipPos(input.positionOS);
    output.uv = TRANSFORM_TEX(input.uv, _AlbedoMap);
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.positionWS = mul(unity_ObjectToWorld,input.positionOS).xyz;
    return output;
}


//在Fragmenet Shader里生成GBuffer
GBufferOutput FragGBuffer(GBufferVaryings input){
    PBRShadeInput pbrShadeInput;
    pbrShadeInput.positionWS = 0;
    pbrShadeInput.albedo = UNITY_SAMPLE_TEX2D(_AlbedoMap,input.uv).rgb * _Color;
    half4 metalInfo = UNITY_SAMPLE_TEX2D(_MetalMap,input.uv);
    pbrShadeInput.metalness = metalInfo.r * _Metalness;
    pbrShadeInput.smooth = metalInfo.a * (1 - _Roughness);
    pbrShadeInput.normal = normalize(input.normalWS);
    pbrShadeInput.positionWS = input.positionWS;
    return EncodePBRInputToGBuffer(pbrShadeInput);
}


#endif
