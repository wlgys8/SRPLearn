#ifndef X_LIGHT_INPUT_INCLUDED
#define X_LIGHT_INPUT_INCLUDED

#include "HLSLSupport.cginc"
#include "./CommonInput.hlsl"

//整个场景最大生效光源数量
#define MAX_OTHER_VISIBLE_LIGHT_COUNT  32 

//单个物体可生效的光源数量
#define MAX_OTHER_LIGHT_PER_OBJECT 8

CBUFFER_START(XLighting)
//环境光
half4 _XAmbientColor;
//主灯光方向
float4 _XMainLightDirection;
//主灯光颜色
half4 _XMainLightColor;

//主灯光 世界空间->投影空间变换矩阵
float4x4 _XMainLightMatrixWorldToShadowMap;

//非主光源的位置和范围,xyz代表位置，w代表范围
float4 _XOtherLightPositionAndRanges[MAX_OTHER_VISIBLE_LIGHT_COUNT];
//非主光源的颜色
half4 _XOtherLightColors[MAX_OTHER_VISIBLE_LIGHT_COUNT];

CBUFFER_END

#define OTHER_LIGHT_COUNT unity_LightData.y

StructuredBuffer<float4> _DeferredOtherLightPositionAndRanges;
StructuredBuffer<half4> _DeferredOtherLightColors;
uniform uint _DeferredOtherLightCount;


int GetOtherLightCount(){
    return clamp(OTHER_LIGHT_COUNT,0,MAX_OTHER_LIGHT_PER_OBJECT);
}

struct XDirLight{
    float3 direction;
    half4 color;
};

struct XOtherLight{
    float4 positionRange;
    half4 color;
};

struct ShadeLightDesc{
    half3 dir;
    half3 color;
};

struct PointLightInteractData{
    half3 lightDir;
    half atten;
};

XDirLight GetMainLight(){
    XDirLight light;
    light.direction = _XMainLightDirection;
    light.color = _XMainLightColor;
    return light;
}


XOtherLight GetOtherLight(int index){
    XOtherLight light;
    int idx = index / 4;
    int offset = index % 4;
    int lightIndex  = unity_LightIndices[idx][offset];
    float4 positionRange = _XOtherLightPositionAndRanges[lightIndex];
    half4 color = _XOtherLightColors[lightIndex];
    light.positionRange = positionRange;
    light.color = color;
    return light;
}





#endif