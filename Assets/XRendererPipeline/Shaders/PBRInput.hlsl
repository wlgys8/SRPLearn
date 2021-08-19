#ifndef X_PBR_INPUT_INCLUDED
#define X_PBR_INPUT_INCLUDED

struct Attributes
{
    float4 positionOS   : POSITION;
    float2 uv           : TEXCOORD0;
    float3 normalOS     : NORMAL;
};

struct Varyings
{
    float2 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
    float3 normalWS    : TEXCOORD1;
    float3 positionWS   : TEXCOORD2;
};

struct GBufferVaryings{
    float2 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
    float3 normalWS    : TEXCOORD1;
    float3 positionWS   : TEXCOORD2;
};

UNITY_DECLARE_TEX2D(_AlbedoMap);
UNITY_DECLARE_TEX2D(_MetalMap);
UNITY_DECLARE_TEX2D(_BRDFLUT);

samplerCUBE _IBLSpec;

CBUFFER_START(UnityPerMaterial)
float _Metalness;
float _Roughness;
half4 _Color;
float4 _AlbedoMap_ST;
uint _IBLSpecMaxMip;
CBUFFER_END

#endif