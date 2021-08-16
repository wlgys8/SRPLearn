#ifndef X_SHADOW_INCLUDED
#define X_SHADOW_INCLUDED

#include "./LightInput.hlsl"
#include "./ShadowInput.hlsl"
#include "./SpaceTransform.hlsl"
#include "./ShadowTentFilter.hlsl"
#include "./ShadowBias.hlsl"

#define ACTIVED_CASCADE_COUNT _ShadowParams.w

struct CascadeShadowData{
    int cascadeIndex;
    float blend;
};
 
int GetCascadeIndex(float3 positionWS){
    for(int i = 0; i < ACTIVED_CASCADE_COUNT; i ++){
        float4 cullingSphere = _XCascadeCullingSpheres[i];
        float3 center = cullingSphere.xyz;
        float radiusSqr = cullingSphere.w * cullingSphere.w;
        float3 d = (positionWS - center);
        //计算世界坐标是否在包围球内。
        if(dot(d,d) <= radiusSqr){
            return i;
        }
    }
    return -1;
}

CascadeShadowData GetCascadeShadowData(float3 positionWS,float3 normalWS){
    CascadeShadowData data;
    data.blend = 0;
    data.cascadeIndex = -1;
    float3 viewVector = _WorldSpaceCameraPos - positionWS;
    float3 cameraForward = _WorldSpaceCameraForward;
    float projOnForward = - dot(viewVector,cameraForward);
    if(projOnForward > SHADOW_DISTANCE){
        //超出最远阴影距离，不渲染阴影
        return data;
    }
    
    //面与摄像机朝向的夹角对其在屏幕上的投影距离也有影响
    // float nf = min(3,1 / abs(dot(cameraForwardUnit,normalWS)));
    
    
    float blendDist = 0; 
    float lastSphereRadius = 0;
    for(int i = 0; i < ACTIVED_CASCADE_COUNT; i ++){
        float4 cullingSphere = _XCascadeCullingSpheres[i];
        if(i < ACTIVED_CASCADE_COUNT - 1){
            float3 center = cullingSphere.xyz;
            float distToCenter = length(positionWS - center);
            float distToSurface = cullingSphere.w - distToCenter;
            //计算世界坐标是否在包围球内。
            if(distToSurface > 0){
                //blendDist应当与距离有关，像素距离摄像机越远，应当给予越长的距离用来混合
                blendDist = CASCADE_SHADOW_BLEND_DIST * projOnForward;
                float3 centerToPos = positionWS - center;
                if(dot(centerToPos,cameraForward) > 0){
                    //如果两级CSM之间的Bias差距越大，那么也需要越长的距离用来混合
                    float deltaBiasScale = 1 + abs(_CascadeShadowBiasScale[i + 1] - _CascadeShadowBiasScale[i]);
                    blendDist *= deltaBiasScale;
                    //最大混合距离不超过CSM包围盒半径的一半
                    blendDist = min(cullingSphere.w * 0.5,blendDist);
                    data.blend =  saturate(1 - distToSurface/blendDist);
                    data.cascadeIndex = i;
                }else{
                    data.cascadeIndex = i;
                    data.blend = 0;
                }
                return data;
            }
        }else{
            blendDist = CASCADE_SHADOW_BLEND_DIST * projOnForward;
            blendDist =  min(cullingSphere.w * 0.5,blendDist);
            data.cascadeIndex = i;
            data.blend = saturate(1 - (SHADOW_DISTANCE - projOnForward) / blendDist);
        }
        lastSphereRadius = cullingSphere.w;
    }
    return data;
}


///将世界坐标转换到ShadowMapTexture空间,返回值的xy为uv，z为深度
float3 WorldToShadowMapPos(float3 positionWS,int cascadeIndex){
    if(cascadeIndex >= 0){
        float4x4 worldToCascadeMatrix = _XWorldToMainLightCascadeShadowMapSpaceMatrices[cascadeIndex];
        float4 shadowMapPos = mul(worldToCascadeMatrix,float4(positionWS,1));
        shadowMapPos /= shadowMapPos.w;
        return shadowMapPos;
    }else{
        //表示超出ShadowMap. 不显示阴影。
        #if UNITY_REVERSED_Z
        return float3(0,0,1);
        #else
        return float3(0,0,0);
        #endif
    }
}

float3 WorldToShadowMapPos(float3 positionWS){
    int cascadeIndex = GetCascadeIndex(positionWS);
    return WorldToShadowMapPos(positionWS,cascadeIndex);
}



///采样阴影强度，返回区间[0,1]
float SampleShadowStrength(float3 uvd){
    #if X_SHADOW_PCF
        float atten = 0;
        if(_ShadowAAParams.x == 1){
            atten = SampleShadowPCF(uvd);
        }else if(_ShadowAAParams.x == 2){
            atten = SampleShadowPCF3x3_4Tap_Fast(uvd);
        }else if(_ShadowAAParams.x == 3){
            atten = SampleShadowPCF3x3_4Tap(uvd);
        }else if(_ShadowAAParams.x == 4){
            atten = SampleShadowPCF5x5_9Tap(uvd);
        }else{
            atten = SampleShadowPCF(uvd);
        }
        return 1 - atten;
    #else
        float depth = _XMainShadowMap.SampleLevel(sampler_XMainShadowMap_point_clamp,uvd.xy,0);
        // float depth = UNITY_SAMPLE_TEX2D(_XMainShadowMap,uvd.xy);
        #if UNITY_REVERSED_Z
        //depth > z
        return step(uvd.z,depth);
        #else   
        return step(depth,uvd.z);
        #endif

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

        #if X_CSM_BLEND
            CascadeShadowData cascadeData = GetCascadeShadowData(positionWS,normalWS);
            int cascadeIndex = cascadeData.cascadeIndex;
            float blend = cascadeData.blend;

            #if X_SHADOW_BIAS_RECEIVER_PIXEL
            float3 biasedPositionWS = ApplyShadowBias(positionWS,normalWS,_XMainLightDirection,cascadeIndex);
            #else
            float3 biasedPositionWS = positionWS;
            #endif
            float3 shadowMapPos = WorldToShadowMapPos(biasedPositionWS,cascadeIndex);
            float shadowStrength = SampleShadowStrength(shadowMapPos);

            if(blend > 0 ){
                if(cascadeIndex < ACTIVED_CASCADE_COUNT - 1){
                    #if X_SHADOW_BIAS_RECEIVER_PIXEL
                    biasedPositionWS = ApplyShadowBias(positionWS,normalWS,_XMainLightDirection,cascadeIndex + 1);
                    #else
                    biasedPositionWS = positionWS;
                    #endif
                    shadowMapPos = WorldToShadowMapPos(biasedPositionWS,cascadeIndex + 1);
                    float s2 = SampleShadowStrength(shadowMapPos);
                    shadowStrength = lerp(shadowStrength,s2,blend);
                }else{
                    shadowStrength = lerp(shadowStrength,0,blend);
                }
            }
        #else
            int cascadeIndex = GetCascadeIndex(positionWS);
            if(cascadeIndex < 0){
                return 1;
            }
            #if X_SHADOW_BIAS_RECEIVER_PIXEL
            positionWS = ApplyShadowBias(positionWS,normalWS,_XMainLightDirection,cascadeIndex);
            #endif
            float3 shadowMapPos = WorldToShadowMapPos(positionWS,cascadeIndex);
            float shadowStrength = SampleShadowStrength(shadowMapPos);
        #endif
        return 1 - shadowStrength * _ShadowParams.z;
    #endif
}



#endif