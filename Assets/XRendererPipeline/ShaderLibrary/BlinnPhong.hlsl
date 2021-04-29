#ifndef X_BLINN_PHONG_INCLUDED
#define X_BLINN_PHONG_INCLUDED

struct BlinnPhongProperty{
    half3 diffuseColor;
    half3 specularColor;
    float shininess;
};

struct BlinnPhongGemo{
    float3 normal;
    float3 viewDir;
    float3 position;
};

//Lambert漫反射
float LambertDiffuse(float3 normal,float3 lightDir){
    return max(0,dot(normal,lightDir));
}

//BlinnPhong高光
float BlinnPhongSpecular(float3 viewDir,float3 normal,float3 lightDir,float shininess){
    float3 halfDir = normalize((viewDir  + lightDir));
    float nh = max(0,dot(halfDir,normal));
    return pow(nh,shininess);
}

//漫反射 + 高光
half4 BlinnPhong(float3 lightDir,BlinnPhongGemo gemo,BlinnPhongProperty property){
    half3 diffuse = LambertDiffuse(gemo.normal,lightDir) * property.diffuseColor;
    half3 specular = BlinnPhongSpecular(gemo.viewDir,gemo.normal,lightDir,property.shininess) * property.specularColor;
    return half4(diffuse + specular,1);
}

#endif