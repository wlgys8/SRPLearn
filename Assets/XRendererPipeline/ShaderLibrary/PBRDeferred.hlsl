#ifndef PBR_DEFRRED_INCLUDED
#define PBR_DEFRRED_INCLUDED

#include "./PBRShading.hlsl"
#include "./GBuffer.hlsl"
#include "./Light.hlsl"

static half3 PackNormal(half3 normal){
    return (normal + 1) * 0.5;
}

static half3 UnpackNormal(half3 rgb){
    return rgb * 2 - 1;
}


GBufferOutput EncodePBRInputToGBuffer(PBRShadeInput pbrInput){
    GBufferOutput o;
    o.GBuffer0 = half4(pbrInput.albedo,pbrInput.metalness);
    o.GBuffer1 = half4(PackNormal(pbrInput.normal),pbrInput.smooth);
    o.GBuffer2 = 0;
    o.GBuffer3 = 0;
    return o;
}


void DecodeGBuffer(inout PBRShadeInput input,half4 gbuffer0,half4 gbuffer1,half4 gbuffer2,half4 gbuffer3){
    input.albedo = gbuffer0.rgb;
    input.metalness = gbuffer0.a;
    input.normal = UnpackNormal(gbuffer1.xyz);
    input.smooth = gbuffer1.w;
}

#endif