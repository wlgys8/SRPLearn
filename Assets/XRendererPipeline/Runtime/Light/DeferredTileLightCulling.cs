using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{
    public class DeferredTileLightCulling
    {
        private ComputeShader _computeShader;
        private CommandBuffer _commandbuffer = new CommandBuffer();
        private ComputeBuffer _tileLightsIndicesBuffer;
        private ComputeBuffer _tileLightsArgsBuffer;

        public DeferredTileLightCulling(ComputeShader computeShader){
            _commandbuffer.name = "DeferredTileLightCulling";
            _computeShader = computeShader;
        }

        private Vector4 BuildZBufferParams(float near,float far){
            var result = new Vector4();
            result.x = 1 - far / near;
            result.y = 1 - result.x;
            result.z = result.x / far;
            result.w = result.y / far;
            return result;
        }


        private void EnsureTileComputeBuffer(int tileCountX,int tileCountY){
            var tileCount = tileCountX * tileCountY;
            var argsBufferSize = tileCount;
            var indicesBufferSize = tileCount * DeferredRPSetting.MaxLightCountPerTile;
            if(_tileLightsIndicesBuffer != null && _tileLightsArgsBuffer.count < argsBufferSize){
                _tileLightsIndicesBuffer.Dispose();
                _tileLightsIndicesBuffer = null;
            }
            if(_tileLightsArgsBuffer == null){
                _tileLightsArgsBuffer = new ComputeBuffer(argsBufferSize,sizeof(int),ComputeBufferType.Default,ComputeBufferMode.Dynamic);
                Shader.SetGlobalBuffer(ShaderConstants.TileLightsArgsBuffer,_tileLightsArgsBuffer);
                _computeShader.SetBuffer(0,ShaderConstants.RWTileLightsArgsBuffer,_tileLightsArgsBuffer);
            }

            if(_tileLightsIndicesBuffer != null && _tileLightsIndicesBuffer.count < indicesBufferSize){
                _tileLightsIndicesBuffer.Dispose();
                _tileLightsIndicesBuffer = null;
            }
            if(_tileLightsIndicesBuffer == null){
                _tileLightsIndicesBuffer = new ComputeBuffer(indicesBufferSize,sizeof(int),ComputeBufferType.Default,ComputeBufferMode.Dynamic);
                Shader.SetGlobalBuffer(ShaderConstants.TileLightsIndicesBuffer,_tileLightsIndicesBuffer);
                _computeShader.SetBuffer(0,ShaderConstants.RWTileLightsIndicesBuffer,_tileLightsIndicesBuffer);
            }
        }

        public void Execute(ScriptableRenderContext context,ref DeferredTileLightCullingParams tileLightCullingParams){
            if(!_computeShader){
                return;
            }
            var camera = tileLightCullingParams.cameraRenderDescription;
            var renderTargetIdentifier = tileLightCullingParams.renderTargetIdentifier;
            var lightShadeByComputeShader = tileLightCullingParams.lightShadeByComputeShader;

            uint tx,ty,tz;
            _computeShader.GetKernelThreadGroupSizes(0,out tx,out ty,out tz);

            var screenWidth = camera.pixelWidth;
            var screenHeight = camera.pixelHeight;
            var threadGroupX = Mathf.CeilToInt(screenWidth * 1f / tx);
            var threadGroupY = Mathf.CeilToInt(screenHeight * 1f / ty);

            _commandbuffer.Clear();
            _commandbuffer.SetGlobalVector(ShaderConstants.DeferredTileParams,new Vector4(tx,ty,threadGroupX,threadGroupY));
            if(lightShadeByComputeShader){
                _commandbuffer.SetComputeTextureParam(_computeShader,0,ShaderConstants.OutTexture,renderTargetIdentifier);
            }else{
                EnsureTileComputeBuffer(threadGroupX,threadGroupY);
            }
            _commandbuffer.SetComputeVectorParam(_computeShader,ShaderConstants.TileCount,new Vector4(threadGroupX,threadGroupY,0,0));
            var cameraComp = camera.camera;
            _commandbuffer.SetComputeMatrixParam(_computeShader,ShaderConstants.CameraMatrixV,cameraComp.transform.worldToLocalMatrix);

            var nearPlaneZ = cameraComp.nearClipPlane;
            var nearPlaneHeight = Mathf.Tan(Mathf.Deg2Rad * cameraComp.fieldOfView  * 0.5f) * 2 * cameraComp.nearClipPlane;
            var nearPlaneWidth = cameraComp.aspect * nearPlaneHeight;

            var zbufferParams = BuildZBufferParams(cameraComp.nearClipPlane,cameraComp.farClipPlane);
            _commandbuffer.SetComputeVectorParam(_computeShader,ShaderConstants.ZBufferParams,zbufferParams);
            _commandbuffer.SetComputeVectorParam(_computeShader, ShaderConstants.CameraNearPlaneLB,new Vector4( - nearPlaneWidth/2,-nearPlaneHeight/2,nearPlaneZ,0));
            var basisH = new Vector2(tx * nearPlaneWidth / screenWidth,0);
            var basisV = new Vector2(0,ty * nearPlaneHeight / screenHeight);
            _commandbuffer.SetComputeVectorParam(_computeShader,ShaderConstants.CameraNearBasisH,basisH);
            _commandbuffer.SetComputeVectorParam(_computeShader,ShaderConstants.CameraNearBasisV,basisV);
            _commandbuffer.DispatchCompute(_computeShader,0,threadGroupX,threadGroupY,1);
            context.ExecuteCommandBuffer(_commandbuffer);

        }

        public void Dispose(){
            if(_tileLightsIndicesBuffer != null){
                _tileLightsIndicesBuffer.Dispose();
                _tileLightsIndicesBuffer = null;
            }
            if(_tileLightsArgsBuffer != null){
                _tileLightsArgsBuffer.Dispose();
                _tileLightsArgsBuffer = null;
            }
        }

        public struct DeferredTileLightCullingParams{
            public CameraRenderDescription cameraRenderDescription;
            public RenderTargetIdentifier renderTargetIdentifier;
            public bool lightShadeByComputeShader;
        }

        public static class ShaderConstants
        {
            public static readonly int TileCount = Shader.PropertyToID("_TileCount");
            public static readonly int CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
            public static readonly int OutTexture = Shader.PropertyToID("_OutTexture");
            public static readonly int ZBufferParams = Shader.PropertyToID("_ZBufferParams");
            public static readonly int CameraMatrixV = Shader.PropertyToID("_CameraMatrixV");
            public static readonly int CameraNearPlaneLB = Shader.PropertyToID("_CameraNearPlaneLB");
            public static readonly int CameraNearBasisH = Shader.PropertyToID("_CameraNearBasisH");
            public static readonly int CameraNearBasisV = Shader.PropertyToID("_CameraNearBasisV");
            public static readonly int RWTileLightsArgsBuffer = Shader.PropertyToID("_RWTileLightsArgsBuffer");
            public static readonly int RWTileLightsIndicesBuffer = Shader.PropertyToID("_RWTileLightsIndicesBuffer");


            //*************以下为全局数据*****************//
            
            public static readonly int TileLightsArgsBuffer = Shader.PropertyToID("_TileLightsArgsBuffer");
            public static readonly int TileLightsIndicesBuffer = Shader.PropertyToID("_TileLightsIndicesBuffer");
            public static readonly int DeferredTileParams = Shader.PropertyToID("_DeferredTileParams");

        }



    
    }
}
