#ifndef X_SUBSURFACE_SCATTERING_INCLUDE
#define X_SUBSURFACE_SCATTERING_INCLUDE


//======= Simple Scattering Approximations ===========//
// reference: 
// https://developer.nvidia.com/gpugems/gpugems/part-iii-materials/chapter-16-real-time-approximations-subsurface-scattering

half3 SSS_SimpleWrapDiffuse(half3 NoL,half3 wrap){
    return max(0, (NoL + wrap) / (1 + wrap));
}

#endif