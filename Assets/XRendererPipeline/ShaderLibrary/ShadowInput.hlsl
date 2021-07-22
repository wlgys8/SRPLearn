#ifndef X_SHADOW_INPUT_INCLUDED
#define X_SHADOW_INPUT_INCLUDED

#define MAX_CASCADESHADOW_COUNT 4

CBUFFER_START(XShadow)

//主灯光 世界空间->投影空间变换矩阵
float4x4 _XWorldToMainLightCascadeShadowMapSpaceMatrices[MAX_CASCADESHADOW_COUNT];

float4 _XCascadeCullingSpheres[MAX_CASCADESHADOW_COUNT];

float4 _ShadowCascadeDistances;

CBUFFER_END



#if X_SHADOW_PCF
Texture2D _XMainShadowMap;
SamplerComparisonState sampler_XMainShadowMap;
half4 _ShadowAAParams; //x is PCF tap count, current support 1 & 4
#else
Texture2D _XMainShadowMap;
SamplerState sampler_XMainShadowMap_point_clamp;
#endif

float4 _ShadowParams; //x is cascade shadow blend dist,z is strength,w is cascadeCount

float4 _ShadowMapSize; //x = 1/shadowMap.width, y = 1/shadowMap.height,z = shadowMap.width,w = shadowMap.height


#define CASCADE_SHADOW_BLEND_DIST _ShadowParams.x

#define SHADOW_DISTANCE _ShadowParams.y

#endif