
#ifndef X_SPACE_TRANSFORM_INCLUDE
#define X_SPACE_TRANSFORM_INCLUDE

#include "./CommonInput.hlsl"

float4 ObjectToHClipPosition(float3 positionOS){
    float4 positionWS = mul(unity_ObjectToWorld,float4(positionOS,1));
    return mul(unity_MatrixVP,positionWS);
}

float3 TransformObjectToWorld(float3 positionOS){
    float4 positionWS = mul(unity_ObjectToWorld,float4(positionOS,1));
    return positionWS.xyz;
}

float3 TransformObjectToWorldNormal(float3 normalOS)
{
    return normalize(mul(normalOS, (float3x3)unity_WorldToObject));
}


float4 TransformWorldToHClip(float3 positionWS){
    return mul(unity_MatrixVP,float4(positionWS,1));
}

#define UnityObjectToClipPos ObjectToHClipPosition

#endif