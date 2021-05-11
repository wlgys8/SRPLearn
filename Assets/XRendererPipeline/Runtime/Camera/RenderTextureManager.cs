using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{
    public class RenderTextureManager 
    {
        private static int _colorTexture = Shader.PropertyToID("ColorTexture");

        public static RenderTargetIdentifier AcquireColorTexture(CommandBuffer commandBuffer,ref CameraRenderDescription cameraRenderDescription){
            commandBuffer.GetTemporaryRT(_colorTexture,cameraRenderDescription.pixelWidth,cameraRenderDescription.pixelHeight,
            16,FilterMode.Point,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Default,cameraRenderDescription.msaaLevel,false);
            return _colorTexture;
        }

        public static void ReleaseAllTempRT(CommandBuffer commandBuffer){
            commandBuffer.ReleaseTemporaryRT(_colorTexture);
        }
    }
}
