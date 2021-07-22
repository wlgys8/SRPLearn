using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{
    internal class CameraUtil
    {
        public static void ConfigShaderProperties(CommandBuffer commandBuffer,Camera camera){
            commandBuffer.Clear();
            commandBuffer.SetGlobalVector(CameraShaderProperties.WorldSpaceCameraForward,camera.transform.forward);
        }

        public static void SortCameras(Camera[] cameras){
            for(var i = 0; i < cameras.Length ; i ++){
                var c = cameras[i];
                if(c.cameraType == CameraType.SceneView){
                    var temp = cameras[cameras.Length - 1];
                    cameras[cameras.Length - 1] = c;
                    cameras[i] = temp;
                    return;
                }
            }
        }
    }

    public static class CameraShaderProperties
    {
        public static readonly int WorldSpaceCameraForward = Shader.PropertyToID("_WorldSpaceCameraForward");
    }
}
