#ifndef X_SHADOW_INCLUDED
#define X_SHADOW_INCLUDED

#include "./LightInput.hlsl"
#include "./ShadowInput.hlsl"

UNITY_DECLARE_TEX2D(_XMainShadowMap);

float4 _ShadowParams; //x is depthBias,y is normal bias,z is strength,w is casadeCount

#define ACTIVED_CASADE_COUNT _ShadowParams.w

///将世界坐标转换到ShadowMapTexture空间,返回值的xy为uv，z为深度
float3 WorldToShadowMapPos(float3 positionWS){
    for(int i = 0; i < ACTIVED_CASADE_COUNT; i ++){
        float4 cullingSphere = _XCasadeCullingSpheres[i];
        float3 center = cullingSphere.xyz;
        float radiusSqr = cullingSphere.w * cullingSphere.w;
        float3 d = (positionWS - center);
        //计算世界坐标是否在包围球内。
        if(dot(d,d) <= radiusSqr){
            //如果是，就利用这一级别的Casade来进行采样
            float4x4 worldToCasadeMatrix = _XWorldToMainLightCasadeShadowMapSpaceMatrices[i];
            float4 shadowMapPos = mul(worldToCasadeMatrix,float4(positionWS,1));
            shadowMapPos /= shadowMapPos.w;
            return shadowMapPos;
        }
    }
    //表示超出ShadowMap. 不显示阴影。
    #if UNITY_REVERSED_Z
    return float3(0,0,1);
    #else
    return float3(0,0,0);
    #endif
}

///检查世界坐标是否位于主灯光的阴影之中(0表示不在阴影中，大于0表示在阴影中,数值代表了阴影强度)
float GetMainLightShadowAtten(float3 positionWS,float3 normalWS){
    #if _RECEIVE_SHADOWS_OFF
        return 0;
    #else
        if(_ShadowParams.z == 0){
            return 0;
        }
        float3 shadowMapPos = WorldToShadowMapPos(positionWS + normalWS * _ShadowParams.y);
        float depthToLight = shadowMapPos.z;
        float2 sampeUV = shadowMapPos.xy;
        float depth = UNITY_SAMPLE_TEX2D(_XMainShadowMap,sampeUV);
        #if UNITY_REVERSED_Z
            // depthToLight < depth 表示在阴影之中
            return clamp(step(depthToLight + _ShadowParams.x,depth), 0,_ShadowParams.z);
        #else
            // depthToLight > depth表示在阴影之中
            return clamp(step(depth,depthToLight - _ShadowParams.x), 0,_ShadowParams.z);
        #endif
    #endif
}


/**
======= Shadow Caster Region =======
**/

struct ShadowCasterAttributes
{
    float4 positionOS   : POSITION;
};

struct ShadowCasterVaryings
{
    float4 positionCS   : SV_POSITION;
};

ShadowCasterVaryings ShadowCasterVertex(ShadowCasterAttributes input)
{
    ShadowCasterVaryings output;
    float4 positionCS = UnityObjectToClipPos(input.positionOS);
    output.positionCS = positionCS;
    return output;
}

half4 ShadowCasterFragment(ShadowCasterVaryings input) : SV_Target
{
    return 0;
}

#endif