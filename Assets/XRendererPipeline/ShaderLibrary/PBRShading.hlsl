#ifndef PBR_SHADING_INCLUDED
#define PBR_SHADING_INCLUDED

#include "./PBR_GGX.hlsl"
#include "./Light.hlsl"

#define DECLARE_SHADE_POINT_DESC(varName,pbrShaderInput) ShadePointDesc varName; \
varName.positionWS = pbrShaderInput.positionWS;\
varName.normalWS = pbrShaderInput.normal

#define DECLARE_PBR_DESC(varName,input) PBRDesc varName = InitPBRDesc(1 - input.smooth,input.metalness,input.albedo)

half3 PBRShadingMainLight(PBRShadeInput input){
    float3 positionWS = input.positionWS;
    float3 normalWS = input.normal;
    float3 viewDir = normalize(_WorldSpaceCameraPos - positionWS);
    //着色点几何信息
    DECLARE_SHADE_POINT_DESC(sPointDesc,input);
    //pbr材质相关
    DECLARE_PBR_DESC(pbrDesc,input);
    //平行光
    ShadeLightDesc mainLightDesc = GetMainLightShadeDescWithShadow(positionWS,normalWS);
    half3 color = PBRShading(pbrDesc,sPointDesc,mainLightDesc);
    return color;
}

half3 PBRShadingDirect(PBRShadeInput input){
    float3 positionWS = input.positionWS;
    float3 normalWS = input.normal;
    float3 viewDir = normalize(_WorldSpaceCameraPos - positionWS);
    
    //着色点几何信息
    DECLARE_SHADE_POINT_DESC(sPointDesc,input);
    //pbr材质相关
    DECLARE_PBR_DESC(pbrDesc,input);
    
    //平行光
    ShadeLightDesc mainLightDesc = GetMainLightShadeDescWithShadow(positionWS,normalWS);
    half3 color = PBRShading(pbrDesc,sPointDesc,mainLightDesc);
    
    // 点光源的辐照度计算
    int lightCount = GetOtherLightCount();
    for(int i = 0; i < lightCount; i ++){
        ShadeLightDesc lightDesc = GetOtherLightShadeDesc(i,positionWS);
        color += PBRShading(pbrDesc,sPointDesc,lightDesc);
    }
    return color;
}

#if PBR_SHADE_DEBUG_ON
half3 PBRShadeDebug(PBRShadeInput input){
    int debugMode = _PBRShadeDebugMode;
    half3 color = 0;
    if(debugMode == 1){ //albedo
        color = shadeInput.albedo;
    }else if(debugMode == 2){ //normal
        color = shadeInput.normal;
    }else if(debugMode == 3){ //position
        color = shadeInput.positionWS / 20;
    }else if(debugMode == 4){//metalness
        color = shadeInput.metalness;
    }else if(debugMode == 5){ //roughness   
        color = 1 - shadeInput.smooth;
    }
    return color;
}
#endif

#endif