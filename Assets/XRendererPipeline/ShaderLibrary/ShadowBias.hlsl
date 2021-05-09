#ifndef X_SHADOW_BIAS_INCLUDED
#define X_SHADOW_BIAS_INCLUDED

float4 _ShadowBias;
float4 _CascadeShadowBiasScale;

float3 ApplyShadowBias(float3 positionWS,float3 normalWS,float3 lightDirection){
    float scale = 1 - clamp(dot(normalWS,lightDirection),0,0.9);
    positionWS += lightDirection * _ShadowBias.x * scale;
    positionWS += normalWS * _ShadowBias.y * scale;
    return positionWS;
}

float3 ApplyShadowBias(float3 positionWS,float3 normalWS,float3 lightDirection,uint cascadeIndex){
    float scale = 1 - clamp(dot(normalWS,lightDirection),0,0.9);
    scale *= _CascadeShadowBiasScale[cascadeIndex];
    positionWS += lightDirection * _ShadowBias.x * scale;
    positionWS += normalWS * _ShadowBias.y * scale;
    return positionWS;
}

float3 ApplyShadowBiasAccurate(float3 positionWS,float3 normalWS,float3 lightDirection,uint cascadeIndex){
    float cos = dot(normalWS,lightDirection);
    float sin = sqrt(1 - cos * cos);
    float tan = min(1,sin / cos); //对于depth bias，要避免过大
    float scale = 1 - clamp(dot(normalWS,lightDirection),0,0.9);
    float depthScale = tan * _CascadeShadowBiasScale[cascadeIndex];
    float normalScale = sin * _CascadeShadowBiasScale[cascadeIndex];
    positionWS += lightDirection * _ShadowBias.x * depthScale;
    positionWS += normalWS * _ShadowBias.y * normalScale;
    return positionWS;
}

float3 ApplyShadowCasterBias(float3 positionWS,float3 normalWS,float3 lightDirection){
    return ApplyShadowBias(positionWS,-normalWS,-lightDirection);
}

#endif