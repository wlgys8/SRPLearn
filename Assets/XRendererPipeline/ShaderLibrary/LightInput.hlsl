#ifndef X_LIGHT_INPUT_INCLUDED
#define X_LIGHT_INPUT_INCLUDED

#include "HLSLSupport.cginc"

CBUFFER_START(UnityLighting)
//环境光
half4 _XAmbientColor;
//主灯光方向
float4 _XMainLightDirection;
//主灯光颜色
half4 _XMainLightColor;

//主灯光 世界空间->投影空间变换矩阵
float4x4 _XMainLightMatrixWorldToShadowMap;

CBUFFER_END

#endif