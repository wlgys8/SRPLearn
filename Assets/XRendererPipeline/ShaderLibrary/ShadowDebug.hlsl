#ifndef X_SHADOW_DEBUG_INCLUDED
#define X_SHADOW_DEBUG_INCLUDED

#include "./SpaceTransform.hlsl"
#include "./Shadow.hlsl"


struct ShadowDebugAttributes
{
    float4 positionOS   : POSITION;
    float3 normalOS: NORMAL;
};

struct ShadowDebugVaryings
{
    float4 positionCS   : SV_POSITION;
    float3 positionWS   : TEXCOORD0;
    float3 normalWS     : TEXCOORD1;
};

float4 _ShadowDebugParams;

#define shadowDebugCascadeOn _ShadowDebugParams.y
#define shadowDebugResolutionOn _ShadowDebugParams.z


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
    output.normalWS = mul(unity_ObjectToWorld, float4( input.normalOS, 0.0 )).xyz;
    return output;
}

static half4 cascadeDebugColors[4] = {
    half4(1,0,0,1),
    half4(0,1,0,1),
    half4(0,0,1,1),
    half4(1,1,0,1),
};

half4 ShadowDebugFragment(ShadowDebugVaryings input) : SV_Target
{
    #if X_CSM_BLEND
    CascadeShadowData cascadeData = GetCascadeShadowData(input.positionWS,input.normalWS);
    int cascadeIndex = cascadeData.cascadeIndex;
    float blend = cascadeData.blend;
    #else
    int cascadeIndex = GetCascadeIndex(input.positionWS);
    #endif
    if(cascadeIndex < 0 ){
        discard;
        return half4(0,0,0,0);
    }
    float3 shadowUVD = WorldToShadowMapPos(input.positionWS,cascadeIndex);
    half resolutionDebug = 1;
    if(shadowDebugResolutionOn){
        resolutionDebug =  DebugShadowResolution(shadowUVD.xy);
    }
    half4 color = cascadeDebugColors[cascadeIndex];
    #if X_CSM_BLEND
    if(blend > 0){
        color = (color + cascadeDebugColors[cascadeIndex + 1])/2;
    }
    #endif
    color *= resolutionDebug;
    color.a = _ShadowDebugParams.x;
    return color;
}



#endif