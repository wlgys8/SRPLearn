#ifndef PBR_DEFRRED_INCLUDED
#define PBR_DEFRRED_INCLUDED

#include "./PBRShading.hlsl"
#include "./GBuffer.hlsl"
#include "./Light.hlsl"

///将normal分量从[-1,1]映射到[0,1]
static half3 PackNormal(half3 normalWS){
    return normalWS * 0.5 + 0.5;
}

//将c的分量从[0,1]映射到[-1,1]
static half3 UnpackNormal(half3 c){
    return c * 2 - 1;
}

static half2 SignNotZero(half2 xy){
    return xy >= 0 ? 1:-1;
}


static half2 PackNormalOct(half3 normalWS){
    half l = dot(abs(normalWS),1); //l = abs(x) + abs(y) + abs(z)
    half3 normalOct = normalWS * rcp(l); //投影到八面体
    if(normalWS.z > 0){ //八面体的上部分投影到xy平面
        return normalOct.xy; 
    }else{ //八面体下部分按对角线翻转投影到xy平面
        return (1 - abs(normalOct.yx)) * SignNotZero(normalOct.xy);
    }
}

static half3 UnpackNormalOct(half2 e){
    half3 v = half3(e.xy,1 - abs(e.x) - abs(e.y));
    if(v.z <= 0){
        v.xy = SignNotZero(v.xy) *(1 - abs(v.yx));
    } 
    return normalize(v);
}


static half2 PackNormalAccurate(half3 normalWS){
    return PackNormalOct(normalWS) * 0.5 + 0.5;
}

static half3 UnpackNormalAccurate(half2 e){
    return UnpackNormalOct(e * 2 - 1);
}


GBufferOutput EncodePBRInputToGBuffer(PBRShadeInput pbrInput){
    GBufferOutput o;
    o.GBuffer0 = half4(pbrInput.albedo,pbrInput.metalness);
    half4 g1,g2;
    #if GBUFFER_ACCURATE_NORMAL
    g1.xy = PackNormalAccurate(pbrInput.normal);
    g2.a = pbrInput.smooth;
    #else
    g1.xyz = PackNormal(pbrInput.normal);
    g1.w = pbrInput.smooth;
    g2 = 0;
    #endif
    o.GBuffer1 = g1;
    o.GBuffer2 = g2;
    o.GBuffer3 = 0;
    return o;
}


void DecodeGBuffer(inout PBRShadeInput input,half4 gbuffer0,half4 gbuffer1,half4 gbuffer2,half4 gbuffer3){
    input.albedo = gbuffer0.rgb;
    input.metalness = gbuffer0.a;
    #if GBUFFER_ACCURATE_NORMAL
    half3 normal = UnpackNormalAccurate(gbuffer1.xy);
    input.normal = normal;
    input.smooth = gbuffer2.a;
    #else
    input.normal = UnpackNormal(gbuffer1.xyz);
    input.smooth = gbuffer1.w;
    #endif
}

#endif