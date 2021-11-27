#ifndef X_GBUFFER_INCLUDED
#define X_GBUFFER_INCLUDED


struct GBufferOutput
{
    half4 GBuffer0 : SV_Target0;
    half4 GBuffer1 : SV_Target1;
    half4 GBuffer2 : SV_Target2;
    half4 GBuffer3 : SV_Target3;
};


Texture2D _GBuffer0;
Texture2D _GBuffer1;
Texture2D _GBuffer2;
Texture2D _GBuffer3;


#endif