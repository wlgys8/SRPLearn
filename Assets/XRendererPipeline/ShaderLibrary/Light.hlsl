#ifndef X_LIGHT_INCLUDED
#define X_LIGHT_INCLUDED

#include "./LightInput.hlsl"
#include "./Shadow.hlsl"
#include "./BlinnPhong.hlsl"


struct PointLightInteractData{
    half3 lightDir;
    half atten;
};

//光强随距离衰减公式
float DistanceAtten(float distanceSqr,float rangeSqr){
    float factor = saturate(1 - distanceSqr * rcp(rangeSqr));
    factor = factor * factor;
    return factor * rcp(max(distanceSqr,0.001));
}

int GetOtherLightCount(){
    return clamp(OTHER_LIGHT_COUNT,0,MAX_OTHER_LIGHT_PER_OBJECT);
}


PointLightInteractData GetPointLightInteractData(XOtherLight light,float3 positionWS){
    PointLightInteractData data;
    float3 lightPosition = light.positionRange.xyz;
    //range是光源的有效范围
    float range = light.positionRange.w;
    float rangeSqr = range * range;
    float3 lightVector = lightPosition - positionWS;
    float3 lightDir = normalize(lightVector);
    float distanceToLightSqr = dot(lightVector,lightVector);
    //距离衰减系数
    float atten = DistanceAtten(distanceToLightSqr,rangeSqr);
    data.lightDir = lightDir;
    data.atten = atten;
    return data;
}

//计算所有点光源的BlinnPhong光照
half4 BlinnPhongPointLights(BlinnPhongGemo gemo,BlinnPhongProperty property){
    half3 color;
    //计算点光源的漫反射项
    int lightCount = GetOtherLightCount();
    float3 viewDir = gemo.viewDir;
    float3 positionWS = gemo.position;
    float3 normal = gemo.normal;
    for(int i = 0; i < lightCount; i ++){
        XOtherLight otherLight = GetOtherLight(i);
        PointLightInteractData interactData = GetPointLightInteractData(otherLight,positionWS);
        //高光项
        half3 specular =  BlinnPhongSpecular(viewDir,normal,interactData.lightDir,property.shininess) * property.specularColor;
        //漫反射项
        half3 diffuse = LambertDiffuse(normal,interactData.lightDir) * property.diffuseColor;
        color += interactData.atten * otherLight.color.rgb *  (diffuse + specular) ;
    }
    return half4(color,1);
}




#endif
