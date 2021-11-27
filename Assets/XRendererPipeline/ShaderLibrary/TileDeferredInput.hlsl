#ifndef X_TILE_DEFERRED_INPUT_INCLUDED
#define X_TILE_DEFERRED_INPUT_INCLUDED

uniform float4 _DeferredTileParams;
StructuredBuffer<uint> _TileLightsArgsBuffer;
StructuredBuffer<uint> _TileLightsIndicesBuffer;

#if DEFERRED_BUFFER_DEBUGON
int _DeferredDebugMode;
#endif

#endif
