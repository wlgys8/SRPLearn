#ifndef X_LIGHT_INCLUDED
#define X_LIGHT_INCLUDED

#include "./LightInput.hlsl"
#include "./Shadow.hlsl"
#include "./BlinnPhong.hlsl"


//光强随距离衰减公式
float DistanceAtten(float distanceSqr,float rangeSqr){
    float factor = saturate(1 - distanceSqr * rcp(rangeSqr));
    factor = factor * factor;
    return factor * rcp(max(distanceSqr,0.001));
}


//计算所有点光源的BlinnPhong光照
half4 BlinnPhongPointLights(BlinnPhongGemo gemo,BlinnPhongProperty property){
    half3 color;
    //计算点光源的漫反射项
    int lightCount = clamp(OTHER_LIGHT_COUNT,0,MAX_OTHER_LIGHT_PER_OBJECT);
    float3 viewDir = gemo.viewDir;
    float3 positionWS = gemo.position;
    float3 normal = gemo.normal;
    for(int i = 0; i < lightCount; i ++){
        XOtherLight otherLight = GetOtherLight(i);
        float3 lightPosition = otherLight.positionRange.xyz;
        //range是光源的有效范围
        float range = otherLight.positionRange.w;
        float rangeSqr = range * range;
        float3 lightVector = lightPosition - positionWS;
        float3 lightDir = normalize(lightVector);
        float distanceToLightSqr = dot(lightVector,lightVector);
        //距离衰减系数
        float atten = DistanceAtten(distanceToLightSqr,rangeSqr);
        //高光项
        half3 specular =  BlinnPhongSpecular(viewDir,normal,lightDir,property.shininess) * property.specularColor;
        //漫反射项
        half3 diffuse = LambertDiffuse(normal,lightDir) * property.diffuseColor;
        color += atten * otherLight.color.rgb *  (diffuse + specular) ;
    }
    return half4(color,1);
}




#endif
