using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{
    internal class CameraUtil
    {
        public static void ConfigShaderProperties(CommandBuffer commandBuffer,Camera camera){
            commandBuffer.SetGlobalVector(CameraShaderProperties.WorldSpaceCameraForward,camera.transform.forward);
            var viewMatrix = camera.worldToCameraMatrix;
            //不知道为什么，第二个参数是false才能正常得到世界坐标
            var projectMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix,false);
            var matrixVP = projectMatrix * viewMatrix;
            var invMatrixVP = matrixVP.inverse;
            commandBuffer.SetGlobalMatrix(CameraShaderProperties.CameraMatrixVPInv,invMatrixVP);
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
        public static readonly int CameraMatrixVPInv = Shader.PropertyToID("_CameraMatrixVPInv");
    }
}
