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
    }
}
