#ifndef COMMON_INPUT_INCLUDE
#define COMMON_INPUT_INCLUDE

#include "HLSLSupport.cginc"

//Unity 内置变量
float3 _WorldSpaceCameraPos;
float4x4 unity_MatrixV;
float4x4 unity_MatrixInvV;
float4x4 unity_MatrixVP;
float4x4 unity_MatrixInvVP;
float4 _ScreenParams;

//管线变量
float3 _WorldSpaceCameraForward;


#define TRANSFORM_TEX(tex, name) ((tex.xy) * name##_ST.xy + name##_ST.zw)

///UnityPerDraw是Unity引起内置约定好的一个CBUFFER,里面的变量名都是约定好的，不能修改
CBUFFER_START(UnityPerDraw)
float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
float4 unity_LODFade; // x is the fade value ranging within [0,1]. y is x quantized into 16 levels
float4 unity_WorldTransformParams; // w is usually 1.0, or -1.0 for odd-negative scale transforms

half4 unity_LightData;
half4 unity_LightIndices[2];

CBUFFER_END





#endif