#ifndef X_SHADOW_INCLUDED
#define X_SHADOW_INCLUDED

#include "./LightInput.hlsl"
#include "./ShadowInput.hlsl"
#include "./SpaceTransform.hlsl"


UNITY_DECLARE_TEX2D(_XMainShadowMap);

float4 _ShadowParams; //x is depthBias,y is normal bias,z is strength,w is cascadeCount
float4 _CascadeShadowBiasScale;

#define SHADOW_DEPTH_BIAS _ShadowParams.x
#define SHADOW_NORMAL_BIAS _ShadowParams.y

float4 _ShadowMapSize; //x = 1/shadowMap.width, y = 1/shadowMap.height


#define ACTIVED_CASCADE_COUNT _ShadowParams.w

///将世界坐标转换到ShadowMapTexture空间,返回值的xy为uv，z为深度
float3 WorldToShadowMapPos(float3 positionWS){
    float biasScales[4] = {_CascadeShadowBiasScale.x,_CascadeShadowBiasScale.y,_CascadeShadowBiasScale.z,_CascadeShadowBiasScale.w};
    for(int i = 0; i < ACTIVED_CASCADE_COUNT; i ++){
        float4 cullingSphere = _XCascadeCullingSpheres[i];
        float3 center = cullingSphere.xyz;
        float radiusSqr = cullingSphere.w * cullingSphere.w;
        float3 d = (positionWS - center);
        //计算世界坐标是否在包围球内。
        if(dot(d,d) <= radiusSqr){
            //如果是，就利用这一级别的Cascade来进行采样
            float4x4 worldToCascadeMatrix = _XWorldToMainLightCascadeShadowMapSpaceMatrices[i];
            float4 shadowMapPos = mul(worldToCascadeMatrix,float4(positionWS,1));
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


///采样阴影强度，返回区间[0,1]
float SampleShadowStrength(float3 uvd){

    float depth = UNITY_SAMPLE_TEX2D(_XMainShadowMap,uvd.xy);
    #if UNITY_REVERSED_Z
    //depth > z
    return step(uvd.z,depth);
    #else   
    return step(depth,uvd.z);
    #endif

}

///检查世界坐标是否位于主灯光的阴影之中(1表示不在阴影中，小于1表示在阴影中,数值代表了阴影衰减)
float GetMainLightShadowAtten(float3 positionWS,float3 normalWS){
    #if _RECEIVE_SHADOWS_OFF
        return 1;
    #else
        if(_ShadowParams.z == 0){
            return 1;
        }
        float3 shadowMapPos = WorldToShadowMapPos(positionWS);
        float shadowStrength = SampleShadowStrength(shadowMapPos);
        return 1 - shadowStrength * _ShadowParams.z;
    #endif
}

#endif