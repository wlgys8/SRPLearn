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

float3 ApplyShadowCasterBias(float3 positionWS,float3 normalWS,float3 lightDirection){
    return ApplyShadowBias(positionWS,-normalWS,-lightDirection);
}

#endif