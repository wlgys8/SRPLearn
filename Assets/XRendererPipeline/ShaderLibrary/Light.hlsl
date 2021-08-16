#ifndef X_LIGHT_INCLUDED
#define X_LIGHT_INCLUDED

#include "./LightInput.hlsl"
#include "./Shadow.hlsl"
#include "./BlinnPhong.hlsl"

//光强随距离衰减公式
float DistanceAtten(float distanceSqr,float rangeSqr){
    float x2 = distanceSqr / rangeSqr;
    float x4 = x2 * x2;
    float oneMinuseX4 = saturate(1 - x4);
    float smooth = oneMinuseX4 * oneMinuseX4;
    return smooth / x2;
}

PointLightInteractData GetPointLightInteractData(float4 lightSphere,float3 positionWS){
    PointLightInteractData data;
    float3 lightPosition = lightSphere.xyz;
    //range是光源的有效范围
    float range = lightSphere.w;
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

PointLightInteractData GetPointLightInteractData(XOtherLight light,float3 positionWS){
    return GetPointLightInteractData(light.positionRange,positionWS);
}

ShadeLightDesc GetMainLightShadeDesc(){
    ShadeLightDesc desc;
    desc.dir = _XMainLightDirection;
    desc.color = _XMainLightColor;
    return desc;
}

ShadeLightDesc GetMainLightShadeDescWithShadow(float3 positionWS,half3 normalWS){
    ShadeLightDesc mainLightDesc =  GetMainLightShadeDesc();
    float shadowAtten = GetMainLightShadowAtten(positionWS,normalWS);
    mainLightDesc.color *= shadowAtten;
    return mainLightDesc;
}

ShadeLightDesc GetPointLightShadeDesc(float4 lightSphere,half3 lightColor,float3 positionWS){
    PointLightInteractData interData = GetPointLightInteractData(lightSphere,positionWS);
    ShadeLightDesc desc;
    desc.dir = interData.lightDir;
    desc.color = lightColor * interData.atten;
    return desc;
}

ShadeLightDesc GetOtherLightShadeDesc(int index,float3 positionWS){
    XOtherLight light = GetOtherLight(index);
    return GetPointLightShadeDesc(light.positionRange,light.color.rgb,positionWS);
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
