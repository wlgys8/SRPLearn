#ifndef X_SHADOW_INPUT_INCLUDED
#define X_SHADOW_INPUT_INCLUDED

#define MAX_CASADESHADOW_COUNT 4

CBUFFER_START(XShadow)

//主灯光 世界空间->投影空间变换矩阵
float4x4 _XWorldToMainLightCasadeShadowMapSpaceMatrices[MAX_CASADESHADOW_COUNT];

float4 _XCasadeCullingSpheres[MAX_CASADESHADOW_COUNT];

CBUFFER_END

#endif