#ifndef X_SHADOW_CASTER_INCLUDED
#define X_SHADOW_CASTER_INCLUDED

#include "./LightInput.hlsl"
#include "./SpaceTransform.hlsl"
#include "./ShadowBias.hlsl"

#define ACCURATE_SHADOW_BIAS 0


#if ACCURATE_SHADOW_BIAS

float3 ApplyShadowBias(float3 positionWS,float3 normalWS,float3 lightDirection){
    float cos = dot(normalWS,lightDirection);
    // float sin = sqrt(1 - cos * cos);
    float sin = length(cross(normalWS,lightDirection));
    float depthScale = max(0.1,sin * rcp(cos));
    float normalScale = saturate(sin * rcp(cos * cos));
    positionWS -= lightDirection * _ShadowBias.x * depthScale;
    positionWS -= normalWS * normalScale * _ShadowBias.y;
    return positionWS;
}

#endif


struct ShadowCasterAttributes
{
    float4 positionOS   : POSITION;
    #if X_SHADOW_BIAS_CASTER_VERTEX
    float4 normalOS     : NORMAL;
    #endif
};

struct ShadowCasterVaryings
{
    float4 positionCS   : SV_POSITION;
};

ShadowCasterVaryings ShadowCasterVertex(ShadowCasterAttributes input)
{
    ShadowCasterVaryings output;
    #if X_SHADOW_BIAS_CASTER_VERTEX 
        float3 positionWS = TransformObjectToWorld(input.positionOS);
        float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
        positionWS = ApplyShadowCasterBias(positionWS,normalWS,_XMainLightDirection);
        output.positionCS = TransformWorldToHClip(positionWS);
    #else
        output.positionCS = UnityObjectToClipPos(input.positionOS);
    #endif
    return output;
}

half4 ShadowCasterFragment(ShadowCasterVaryings input) : SV_Target
{
    return 0;
}

#endif