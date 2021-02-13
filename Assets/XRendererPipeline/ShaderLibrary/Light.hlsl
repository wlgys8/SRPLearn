#ifndef X_LIGHT_INCLUDED
#define X_LIGHT_INCLUDED

#include "./LightInput.hlsl"


//Lambert漫反射
half4 LambertDiffuse(float3 normal){
    return max(0,dot(normal,_XMainLightDirection.xyz)) * _XMainLightColor;
}

//BlinnPong光照模型的高光部分
half4 BlinnPhongSpecular(float3 viewDir,float3 normal,float shininess){
    float3 halfDir = normalize((viewDir  + _XMainLightDirection));
    float nh = max(0,dot(halfDir,normal));
    return pow(nh,shininess) * _XMainLightColor;
}

//BlinnPong光照模型
half4 BlinnPongLight(float3 positionWS,float3 normalWS,float shininess,half4 diffuseColor,half4 specularColor){
    float3 viewDir = normalize( _WorldSpaceCameraPos - positionWS);
    return _XAmbientColor + LambertDiffuse(normalWS) * diffuseColor + BlinnPhongSpecular(viewDir,normalWS,shininess) * specularColor; 
}

#endif
