using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{
    public class RenderTextureManager 
    {
        private static int _colorTexture = Shader.PropertyToID("ColorTexture");

        public static RenderTargetIdentifier AcquireColorTexture(CommandBuffer commandBuffer,ref CameraRenderDescription cameraRenderDescription){
            RenderTextureDescriptor renderTextureDescriptor = new RenderTextureDescriptor(cameraRenderDescription.pixelWidth,cameraRenderDescription.pixelHeight);
            renderTextureDescriptor.depthBufferBits = 32;
            renderTextureDescriptor.sRGB = true;
            renderTextureDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            renderTextureDescriptor.msaaSamples = cameraRenderDescription.msaaLevel;
            renderTextureDescriptor.enableRandomWrite = false;
            renderTextureDescriptor.memoryless = RenderTextureMemoryless.MSAA;
            commandBuffer.GetTemporaryRT(_colorTexture,renderTextureDescriptor,FilterMode.Bilinear);
            return _colorTexture;
        }

        public static void ReleaseAllTempRT(CommandBuffer commandBuffer){
            commandBuffer.ReleaseTemporaryRT(_colorTexture);
        }
    }
}
