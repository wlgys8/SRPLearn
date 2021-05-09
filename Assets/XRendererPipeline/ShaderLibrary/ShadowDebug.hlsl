#ifndef X_SHADOW_DEBUG_INCLUDED
#define X_SHADOW_DEBUG_INCLUDED

#include "./SpaceTransform.hlsl"
#include "./Shadow.hlsl"


struct ShadowDebugAttributes
{
    float4 positionOS   : POSITION;
};

struct ShadowDebugVaryings
{
    float4 positionCS   : SV_POSITION;
    float3 positionWS   : TEXCOORD0;
};


///来用显示shadowmap投影在物体上的分辨率。返回1或0
float DebugShadowResolution(float2 uv){
    float2 texelCoord = _ShadowMapSize.zw * uv;
    float2 texelOriginal = floor(texelCoord);
    return abs((texelOriginal.x % 2 + texelOriginal.y % 2)  -1);
}

ShadowDebugVaryings ShadowDebugVertex(ShadowDebugAttributes input)
{
    ShadowDebugVaryings output;
    output.positionCS = UnityObjectToClipPos(input.positionOS);
    output.positionWS = TransformObjectToWorld(input.positionOS);
    return output;
}

half4 ShadowDebugFragment(ShadowDebugVaryings input) : SV_Target
{
    float3 shadowUVD = WorldToShadowMapPos(input.positionWS);
    half d = DebugShadowResolution(shadowUVD.xy);
    return half4(d,0,0,0.2);
}



#endif