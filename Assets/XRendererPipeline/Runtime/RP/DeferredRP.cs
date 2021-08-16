using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace SRPLearn{

    using FrameBufferOutputDebug = DeferredRPSetting.FrameBufferOutputDebug;

    public class DeferredRP : BaseRP
    {
        private const string LightModeId = "Deferred";
        private RenderObjectPass _opaquePass = new RenderObjectPass(false,LightModeId,false);
       
        private ShadowCasterPass _shadowCastPass = new ShadowCasterPass();
        private DeferredLightingPass _deferredLightingPass = new DeferredLightingPass();
        private DeferredTileLightCulling _deferredLightingCulling;
        private DeferredLightConfigurator _deferredLightConfigurator = new DeferredLightConfigurator();

        private List<RenderTexture> _GBuffers = new List<RenderTexture>();
        private RenderTargetIdentifier[] _GBufferRTIs;
        private int[] _GBufferNameIDs = {
            ShaderConstants.GBuffer0,
            ShaderConstants.GBuffer1,
            ShaderConstants.GBuffer2,
            ShaderConstants.GBuffer3,
        };
        private RenderTexture _colorTexture;
        private RenderTexture _depthTexture;

        private BlitPass _blitPass = new BlitPass();
        private FrameBufferOutputDebug? _currentOutputDebug;

        public DeferredRP(XRendererPipelineAsset setting):base(setting){
            _deferredLightingCulling = new DeferredTileLightCulling(setting.builtinAssets.deferredLightingCullingCS);
        }


        private void ConfigShaderPropertiesPerCamera(ScriptableRenderContext context,Camera camera){
            _commandbuffer.Clear();
            CameraUtil.ConfigShaderProperties(_commandbuffer,camera);
            AntiAliasUtil.ConfigShaderPerCamera(_commandbuffer,_setting.antiAliasSetting);
            context.ExecuteCommandBuffer(_commandbuffer);
        }

        private RenderTargetIdentifier AcquireColorTextureIfNot(ScriptableRenderContext context, Camera camera,bool enableRandomWrite=false){

            if(_colorTexture){
                if(_colorTexture.width != camera.pixelWidth || 
                _colorTexture.height != camera.pixelHeight||
                _colorTexture.enableRandomWrite != enableRandomWrite){
                    this.ReleaseColorTexture();
                }
            }

            if(_colorTexture == null){
                RenderTextureDescriptor descriptor = new RenderTextureDescriptor(camera.pixelWidth,camera.pixelHeight);
                descriptor.depthBufferBits = 0;
                descriptor.sRGB = true;
                descriptor.colorFormat = RenderTextureFormat.ARGB32;
                descriptor.enableRandomWrite = enableRandomWrite;
                _colorTexture = RenderTexture.GetTemporary(descriptor);
                _colorTexture.filterMode = FilterMode.Bilinear;
                _colorTexture.Create();
                _commandbuffer.Clear();
                _commandbuffer.SetGlobalTexture(ShaderConstants.CameraColorTexture,_colorTexture);
                context.ExecuteCommandBuffer(_commandbuffer);
            }
            return _colorTexture;
        }

        private void ReleaseColorTexture(){
            if(_colorTexture){
                RenderTexture.ReleaseTemporary(_colorTexture);
                _colorTexture = null;
            }
        }
        protected override void ConfigShaderPropertiesPipeline(ScriptableRenderContext context)
        {
            base.ConfigShaderPropertiesPipeline(context);
            var deferredSetting = _setting.deferredRPSetting;

            ChangeDeferredDebugKeyword(context,deferredSetting.outputDebug);
            _commandbuffer.Clear();
            if(deferredSetting.lightShadeByComputeShader){
                _commandbuffer.EnableShaderKeyword(ShaderKeywords.lightShadeByCS);
            }else{
                _commandbuffer.DisableShaderKeyword(ShaderKeywords.lightShadeByCS);
            }
            Utils.SetGlobalShaderKeyword(_commandbuffer,ShaderKeywords.deferredLightCullingDepthSlice,deferredSetting.enableDepthSliceForLightCulling);
            Utils.SetGlobalShaderKeyword(_commandbuffer,ShaderKeywords.deferredLightCullingMode_SphereBounds,deferredSetting.tileLightCullingAlgorithm == DeferredRPSetting.TileLightCullingAlgorithm.SphereBounds);
            Utils.SetGlobalShaderKeyword(_commandbuffer,ShaderKeywords.deferredLightCullingMode_SideFaces,deferredSetting.tileLightCullingAlgorithm == DeferredRPSetting.TileLightCullingAlgorithm.SideFace);

            context.ExecuteCommandBuffer(_commandbuffer);
        }

        

        private void ChangeDeferredDebugKeyword(ScriptableRenderContext context, FrameBufferOutputDebug outputDebug){
            if(_currentOutputDebug == outputDebug){
                return;
            }
            _currentOutputDebug = outputDebug;
            _commandbuffer.Clear();
            if(outputDebug != FrameBufferOutputDebug.Off){
                _commandbuffer.EnableShaderKeyword(ShaderKeywords.deferredDebugOn);
            }else{
                _commandbuffer.DisableShaderKeyword(ShaderKeywords.deferredDebugOn);
            }
            _commandbuffer.SetGlobalInt(ShaderConstants.DeferredDebugMode,(int)outputDebug);
            context.ExecuteCommandBuffer(_commandbuffer);
        }

        protected override void OnPostCameraCulling(ScriptableRenderContext context, Camera camera, ref CullingResults cullingResults)
        {
            base.OnPostCameraCulling(context, camera, ref cullingResults);
            //生成延迟着色需要的灯光数据
            var lightData = _deferredLightConfigurator.Prepare(ref cullingResults);

            //投影Pass
            var casterSetting = new ShadowCasterPass.ShadowCasterSetting();
            casterSetting.cullingResults = cullingResults;
            casterSetting.lightData = lightData;
            casterSetting.shadowSetting = _setting.shadowSetting;
            casterSetting.camera = camera;
            _shadowCastPass.Execute(context,ref casterSetting);

            //重设摄像机参数
            context.SetupCameraProperties(camera);
            ConfigShaderPropertiesPerCamera(context,camera);

            //设置MRT
            var cameraDesc = Utils.GetCameraRenderDescription(camera,_setting);
            this.ConfigMRT(context,ref cameraDesc);

            //渲染非透明物体
            _opaquePass.Execute(context,camera,ref cullingResults);

            var lightShadeByComputeShader = _setting.deferredRPSetting.lightShadeByComputeShader; //是否直接使用compute shader进行光照着色
            var isFXAAOn = _setting.antiAliasSetting.isFXAAOn;
            //如果使用CS着色、或者开启FXAA，那么需要自己申请一张RT
            if(lightShadeByComputeShader){
                this.AcquireColorTextureIfNot(context,camera,true);
            }else if(isFXAAOn){
                this.AcquireColorTextureIfNot(context,camera,false);
            }else{
                this.ReleaseColorTexture();
            }

            var deferredTileLightCullingParams = new DeferredTileLightCulling.DeferredTileLightCullingParams(){
                cameraRenderDescription = cameraDesc,
                renderTargetIdentifier = _colorTexture,
                lightShadeByComputeShader = lightShadeByComputeShader
            };
            _deferredLightingCulling.Execute(context,ref deferredTileLightCullingParams);

            if(lightShadeByComputeShader){
                //如果光照是从CS计算的，需要一个Blit操作将图形从RT拷贝到设备屏幕
                _blitPass.Config(_colorTexture,BuiltinRenderTextureType.CameraTarget);
                _blitPass.Execute(context);
            }else{
                //如果光照不从CS计算

                //如果开启FXAA，那么LightPass也需要先渲染到RT上
                if(isFXAAOn){
                    _commandbuffer.Clear();
                    _commandbuffer.SetRenderTarget(_colorTexture);
                    context.ExecuteCommandBuffer(_commandbuffer);
                }else{
                    //从MRT换回CameraTarget
                    _commandbuffer.Clear();
                    _commandbuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                    context.ExecuteCommandBuffer(_commandbuffer);
                }
                _deferredLightingPass.Execute(context);
                if(isFXAAOn){
                    _commandbuffer.Clear();
                    _commandbuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                    context.ExecuteCommandBuffer(_commandbuffer);
                    _blitPass.Config(_colorTexture,BuiltinRenderTextureType.CameraTarget);
                    _blitPass.Execute(context);
                }
            }
            #if UNITY_EDITOR
            if(camera.cameraType == CameraType.SceneView && UnityEditor.Handles.ShouldRenderGizmos()){
                context.DrawGizmos(camera,GizmoSubset.PostImageEffects);
            }
            #endif
            OnCameraRenderingEnd(context);
        }


        private void ReleaseGBuffers(){
            if(_GBuffers.Count > 0){
                foreach(var g in _GBuffers){
                    if(g){
                        RenderTexture.ReleaseTemporary(g);
                    }
                }
                _GBuffers.Clear();
                _GBufferRTIs = null;
            }
        }

        private void AcquireGBuffersIfNot(ScriptableRenderContext context, Camera camera){
            if(_GBuffers.Count > 0){
                var g0 = _GBuffers[0];
                if(g0.width != camera.pixelWidth || g0.height != camera.pixelHeight){
                    this.ReleaseGBuffers();
                }
            }
            if(_GBuffers.Count == 0){
                _commandbuffer.Clear();
                _GBufferRTIs = new RenderTargetIdentifier[4];
                for(var i = 0; i < 4; i ++){
                    RenderTextureDescriptor descriptor = new RenderTextureDescriptor(camera.pixelWidth,camera.pixelHeight,RenderTextureFormat.ARGB32,0,1);
                    var rt = RenderTexture.GetTemporary(descriptor);
                    rt.filterMode = FilterMode.Bilinear;
                    rt.Create();
                    _GBuffers.Add(rt);
                    _commandbuffer.SetGlobalTexture(_GBufferNameIDs[i],rt);
                    _GBufferRTIs[i] = rt;
                }
                context.ExecuteCommandBuffer(_commandbuffer);
            }
        }
        private void ReleaseDepthTexture(){
            if(_depthTexture){
                 RenderTexture.ReleaseTemporary(_depthTexture);
                _depthTexture = null;
            }
        }
        private void AcquireDepthTextureIfNot(ScriptableRenderContext context, Camera camera){
            if(_depthTexture){
                if(_depthTexture.width != camera.pixelWidth || _depthTexture.height != camera.pixelHeight){
                    this.ReleaseDepthTexture();
                }
            }
            if(!_depthTexture){
                RenderTextureDescriptor depthDesc = new RenderTextureDescriptor(camera.pixelWidth,camera.pixelHeight,RenderTextureFormat.Depth,32,1);
                _depthTexture = RenderTexture.GetTemporary(depthDesc);
                _depthTexture.Create();
                _commandbuffer.Clear();
                _commandbuffer.SetGlobalTexture(ShaderConstants.CameraDepthTexture,_depthTexture);
                context.ExecuteCommandBuffer(_commandbuffer);
            }
        }

        private void ConfigMRT(ScriptableRenderContext context,ref CameraRenderDescription cameraRenderDescription){
            this.AcquireGBuffersIfNot(context,cameraRenderDescription.camera);
            this.AcquireDepthTextureIfNot(context,cameraRenderDescription.camera);
            _commandbuffer.Clear();
            _commandbuffer.SetRenderTarget(_GBufferRTIs,_depthTexture);
            _commandbuffer.ClearRenderTarget(true,true,cameraRenderDescription.camera.backgroundColor);
            context.ExecuteCommandBuffer(_commandbuffer);
        }

        private void OnCameraRenderingEnd(ScriptableRenderContext context){
      
        }


        protected override void Dispose(bool disposing)
        {
            Debug.Log("DeferredRP Dispose");
            base.Dispose(disposing);
            _deferredLightConfigurator.Dispose();
            _deferredLightingCulling.Dispose();
            this.ReleaseColorTexture();
            this.ReleaseGBuffers();
            this.ReleaseDepthTexture();
        }


        public static class ShaderConstants
        {
            public static readonly int GBuffer0 = Shader.PropertyToID("_GBuffer0");
            public static readonly int GBuffer1 = Shader.PropertyToID("_GBuffer1");
            public static readonly int GBuffer2 = Shader.PropertyToID("_GBuffer2");
            public static readonly int GBuffer3 = Shader.PropertyToID("_GBuffer3");

            public static readonly int CameraColorTexture = Shader.PropertyToID("_CameraColorTexture");

            public static readonly int CameraDepthTexture = Shader.PropertyToID("_XDepthTexture");
            public static readonly int DeferredDebugMode = Shader.PropertyToID("_DeferredDebugMode");

            public static readonly int TileCullingIntersectAlgroThreshold = Shader.PropertyToID("_TileCullingIntersectAlgroThreshold");
        }

        public static class ShaderKeywords
        {

            public const string lightShadeByCS = "DEFERRED_LIGHTSHADE_BY_CS";
            public const string deferredDebugOn = "DEFERRED_BUFFER_DEBUGON";
            public const string deferredLightCullingDepthSlice = "DEFERRED_LIGHT_CULLING_DEPTH_SLICE";
            public const string deferredLightCullingMode_SideFaces = "DEFERRED_LIGHT_CULLING_SIDES";
            public const string deferredLightCullingMode_SphereBounds = "DEFERRED_LIGHT_CULLING_SPHERE_BOUNDS";
        }
    }
}
