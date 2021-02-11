#include "HLSLSupport.cginc"
CBUFFER_START(UnityLighting)
//环境光
half4 _XAmbientColor;
//主灯光方向
float4 _XMainLightDirection;
//主灯光颜色
half4 _XMainLightColor;

CBUFFER_END


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

