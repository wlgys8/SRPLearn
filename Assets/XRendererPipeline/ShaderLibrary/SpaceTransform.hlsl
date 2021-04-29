
#ifndef X_SPACE_TRANSFORM_INCLUDE
#define X_SPACE_TRANSFORM_INCLUDE

#include "./CommonInput.hlsl"

float4 ObjectToHClipPosition(float3 positionOS){
    float4 positionWS = mul(unity_ObjectToWorld,float4(positionOS,1));
    return mul(unity_MatrixVP,positionWS);
}

#define UnityObjectToClipPos ObjectToHClipPosition

#endif