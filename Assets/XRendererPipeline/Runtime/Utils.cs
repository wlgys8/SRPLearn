using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{
    public static class Utils
    {
        
        public static void SetGlobalShaderKeyword(CommandBuffer commandBuffer,string keyword,bool enable){
            if(enable){
                commandBuffer.EnableShaderKeyword(keyword);
            }else{
                commandBuffer.DisableShaderKeyword(keyword);
            }
        }


        public static CameraRenderDescription GetCameraRenderDescription(Camera camera,XRendererPipelineAsset asset){
            var aa = asset.antiAliasSetting;
            CameraRenderDescription des = new CameraRenderDescription(camera);
            bool msaaEnable = aa.antiAliasType == AAType.MSAA && camera.allowMSAA;
            des.requireTempRT = msaaEnable || aa.antiAliasType == AAType.FXAAQuality || aa.antiAliasType == AAType.FXAAConsole || aa.antiAliasType == AAType.FXAAV1;
            if(msaaEnable){
                des.msaaLevel = aa.msaaLevel;
            }
            return des;
        }


    }
}
