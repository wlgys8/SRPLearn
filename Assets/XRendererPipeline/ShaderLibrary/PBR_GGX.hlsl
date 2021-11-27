#ifndef PBR_GGX_INCLUDED
#define PBR_GGX_INCLUDED

#define INV_PI 0.31830989161357

#include "./LightInput.hlsl"

//着色点的几何信息
struct ShadePointDesc{
    float3 positionWS;
    float3 normalWS;
};

//PBR材质信息
struct PBRDesc{
    half roughness; //粗糙度
    half metalness;//金属度
    half3 albedo;//漫反射系数/反射率 
    half a; // a = roughness^2
    half a2; // a2 = a^2
    half k; //k = 0.5a
    half oneMinusK; //1 - k
    half3 f0; //菲涅尔反射的f0
};


struct BRDFData{
    half3 h; //h = normalize(l + v)
    half NoL; //dot(n,l)
    half NoV;//dot(n,v)
    half NoH;//dot(n,h)
    half VoH; //dot(v,h)
};

struct PBRShadeInput{
    half3 albedo;
    half metalness;
    half smooth;
    half3 normal;
    float3 positionWS;
};

PBRDesc InitPBRDesc(half roughness,half metalness,half3 albedo){
    PBRDesc desc;
    desc.roughness = roughness;
    desc.metalness = metalness;
    desc.albedo = albedo;
    desc.a = roughness * roughness;
    desc.a2 = desc.a * desc.a;
    desc.k = desc.a * 0.5;
    desc.oneMinusK = 1 - desc.k;
    desc.f0 = lerp(0.04,desc.albedo,desc.metalness);
    return desc;
};

void InitializeBRDFData(half3 l,half3 v,half3 n,inout BRDFData brdfData){
    half3 h = normalize(l + v);
    brdfData.h = h;
    brdfData.NoL = max(0,dot(n,l));
    brdfData.NoV = max(0,dot(n,v));
    brdfData.NoH = max(0,dot(n,h));
    brdfData.VoH = max(0,dot(v,h));
}

BRDFData InitializeBRDFData(ShadePointDesc pointDesc,ShadeLightDesc lightDesc){
    half3 v = normalize(_WorldSpaceCameraPos - pointDesc.positionWS);
    half3 l = lightDesc.dir;
    half3 n = pointDesc.normalWS;
    BRDFData brdfData;
    half3 h = normalize(l + v);
    brdfData.h = h;
    brdfData.NoL = max(0,dot(n,l));
    brdfData.NoV = max(0,dot(n,v));
    brdfData.NoH = max(0,dot(n,h));
    brdfData.VoH = max(0,dot(v,h));
    return brdfData;
}


//法线分布函数
float D_TrowbridgeReitzGGX(half a2,half NoH){
    half nh2 = NoH * NoH;
    half b = nh2 * (a2 - 1) + 1.00001f;
    return a2 * INV_PI / (b * b);
}

//这是一个优化,因为CookBRDF的分母中有ndotl和ndotv，把G函数的分子中也有ndotl和ndotv。因此可以直接约去，合并成V项
float V_SmithGGX(PBRDesc desc,BRDFData brdfData){
    half oneMinusK = desc.oneMinusK;
    half k = desc.k;
    return rcp(brdfData.NoV * oneMinusK + k) * rcp(brdfData.NoL * oneMinusK + k);
}

//菲涅尔函数
half3 F_Schlick(half3 f0,half VoH){
    return f0 + (1 - f0) * pow(1 - VoH,5);
}

//加入粗糙度的菲涅尔函数，针对粗糙表面，会减弱其在边缘的反射率(ks)
half3 F_SchlickRoughness(half3 f0,float NoV,float roughness){
    return f0 + (max(1.0 - roughness, f0) - f0) * pow(1.0 - NoV, 5.0);
}

half3 BRDF(PBRDesc desc,BRDFData brdfData){
    half3 F = F_Schlick(desc.f0,brdfData.VoH);
    float D = D_TrowbridgeReitzGGX(desc.a2,brdfData.NoH);
    float V = V_SmithGGX(desc,brdfData);
    half3 specular = D * V * F * 0.25;
    half3 ks = F;
    half3 kd = (1 - ks)*(1 - desc.metalness); //金属没有漫反射
    return kd * INV_PI * desc.albedo + ks * specular;
}

half3 BRDFIBLSpec(PBRDesc desc, float2 scaleBias){
    return desc.f0 * scaleBias.x + scaleBias.y;
}


half3 PBRShading(PBRDesc pbrDesc,ShadePointDesc pointDesc,ShadeLightDesc lightDesc){
    BRDFData brdfData = InitializeBRDFData(pointDesc,lightDesc);
    half3 irradiance = brdfData.NoL * lightDesc.color;
    half3 color = BRDF(pbrDesc,brdfData) * irradiance;
    return color;
}



#endif




