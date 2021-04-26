#ifndef X_SHADOW_INPUT_INCLUDED
#define X_SHADOW_INPUT_INCLUDED

#define MAX_CASCADESHADOW_COUNT 4

CBUFFER_START(XShadow)

//主灯光 世界空间->投影空间变换矩阵
float4x4 _XWorldToMainLightCascadeShadowMapSpaceMatrices[MAX_CASCADESHADOW_COUNT];

float4 _XCascadeCullingSpheres[MAX_CASCADESHADOW_COUNT];

CBUFFER_END

#endif