using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

namespace SRPLearn{


    [CreateAssetMenu(menuName = "SRPLearn/XRendererPipelineAsset")]
    public class XRendererPipelineAsset : RenderPipelineAsset
    {
        [SerializeField]
        private bool _srpBatcher = true;

        [SerializeField]
        private ShadowSetting _shadowSetting = new ShadowSetting();

        [SerializeField]
        private AntiAliasSetting _antiAlias = new AntiAliasSetting();

        [SerializeField]
        private BuiltinAssets _builtinAssets;

        public BuiltinAssets builtinAssets{
            get{
                return _builtinAssets;
            }
        }

        public bool enableSrpBatcher{
            get{
                return _srpBatcher;
            }
        }

        public ShadowSetting shadowSetting{
            get{
                return _shadowSetting;
            }
        }

        public AntiAliasSetting antiAliasSetting{
            get{
                return _antiAlias;
            }
        }

        protected override RenderPipeline CreatePipeline()
        {
            return new XRenderPipeline(this);
        }
    }


    public class XRenderPipeline : RenderPipeline
    {
        public static event System.Action onPipelineBegin;

        public static event System.Action onPipelineEnd;

        private ShaderTagId _shaderTag = new ShaderTagId("XForwardBase");
        private LightConfigurator _lightConfigurator = new LightConfigurator();

        private RenderObjectPass _opaquePass = new RenderObjectPass(false);
        private RenderObjectPass _transparentPass = new RenderObjectPass(true);

        private ShadowCasterPass _shadowCastPass = new ShadowCasterPass();
        private RenderObjectPass _shadowDebugPass = new RenderObjectPass(false,"ShadowDebug");
        private CommandBuffer _command = new CommandBuffer();
        private XRendererPipelineAsset _setting;

        private BlitPass _blitPass = new BlitPass();

        private RenderTargetIdentifier _currentColorTarget;
        private RenderTargetIdentifier _currentDepthTarget;


        public XRenderPipeline(XRendererPipelineAsset setting){
            GraphicsSettings.useScriptableRenderPipelineBatching = setting.enableSrpBatcher;
            _command.name = "RenderCamera";
            _setting = setting;
            Shader.SetGlobalTexture("_BRDFLUT",setting.builtinAssets.BRDFLUT);
        }

        private void ConfigPipelineShaderKeywords(){
            if(_setting.shadowSetting.isCSMBlendEnabled){
                Shader.EnableKeyword(ShadowCasterPass.ShaderKeywords.CSMBlend);
            }else{
                Shader.DisableKeyword(ShadowCasterPass.ShaderKeywords.CSMBlend);
            }
        }

        private void ConfigPipelineShaderProeprties(ScriptableRenderContext context){
            ShadowUtils.ConfigCascadeDistances(_command,_setting.shadowSetting);
            context.ExecuteCommandBuffer(_command);
        }

        private void OnPipelineBegin(){
            if(onPipelineBegin != null){
                onPipelineBegin();
            }
            ShadowDebug.Setup(_setting.shadowSetting);
        }

        private void OnPipelineEnd(){
            if(onPipelineEnd != null){
                onPipelineEnd();
            }
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            OnPipelineBegin();
            this.ConfigPipelineShaderKeywords();
            this.ConfigPipelineShaderProeprties(context);
            CameraUtil.SortCameras(cameras);
            //遍历摄像机，进行渲染
            foreach(var camera in cameras){
                RenderPerCamera(context,camera);
            }
            //提交渲染命令
            context.Submit();
            OnPipelineEnd();
        }

        private void ConfigRenderTarget(ScriptableRenderContext context,ref CameraRenderDescription cameraRenderDescription){
            _command.Clear();
            if(cameraRenderDescription.requireTempRT){
                _currentColorTarget = RenderTextureManager.AcquireColorTexture(_command,ref cameraRenderDescription);
                _currentDepthTarget = _currentColorTarget;
            }else{
                _currentColorTarget = BuiltinRenderTextureType.CameraTarget;
                _currentDepthTarget = _currentColorTarget;
            }
            _command.SetRenderTarget(_currentColorTarget,_currentDepthTarget);
            _command.ClearRenderTarget(true,true,cameraRenderDescription.camera.backgroundColor);
            context.ExecuteCommandBuffer(_command);
        }

        private void ConfigShaderPerCamera(ScriptableRenderContext context,Camera camera){
            AntiAliasUtil.ConfigShaderPerCamera(context,_command,_setting.antiAliasSetting);
            CameraUtil.ConfigShaderProperties(_command,camera);
            context.ExecuteCommandBuffer(_command);
            _command.Clear();
        }

        private void ConfigShadowDebugParams(ScriptableRenderContext context){
            var shadowSetting = _setting.shadowSetting;
            ShadowUtils.ConfigShadowDebugParams(_command,shadowSetting);
            context.ExecuteCommandBuffer(_command);
        }

        private void RenderPerCamera(ScriptableRenderContext context,Camera camera){
            //设置摄像机参数
            context.SetupCameraProperties(camera);
            //对场景进行裁剪
            camera.TryGetCullingParameters( out var cullingParams);

            cullingParams.shadowDistance = Mathf.Min(_setting.shadowSetting.shadowDistance,camera.farClipPlane - camera.nearClipPlane);

            var cullingResults = context.Cull(ref cullingParams);

            var lightData = _lightConfigurator.SetupShaderLightingParams(context,ref cullingResults);

            var casterSetting = new ShadowCasterPass.ShadowCasterSetting();
            casterSetting.cullingResults = cullingResults;
            casterSetting.lightData = lightData;
            casterSetting.shadowSetting = _setting.shadowSetting;
            casterSetting.camera = camera;
            //投影Pass
            _shadowCastPass.Execute(context,ref casterSetting);

            //重设摄像机参数
            context.SetupCameraProperties(camera);

            var cameraDescription = Utils.GetCameraRenderDescription(camera,_setting);

            //重新配置渲染目标
            ConfigRenderTarget(context,ref cameraDescription);

            //设置keywords
            ConfigShaderPerCamera(context,camera);

            //非透明物体渲染
            _opaquePass.Execute(context,camera,ref cullingResults);

            //透明物体渲染
            _transparentPass.Execute(context,camera,ref cullingResults);

            if(this._setting.shadowSetting.shouldShadowDebugPassOn){
                //阴影调试
                this.ConfigShadowDebugParams(context);
                _shadowDebugPass.Execute(context,camera,ref cullingResults);
            }

            if(camera.cameraType == CameraType.SceneView){
                context.DrawGizmos(camera,GizmoSubset.PostImageEffects);
            }



            //final blit
            if(_currentColorTarget != BuiltinRenderTextureType.CameraTarget){
                _blitPass.Config(_currentColorTarget,BuiltinRenderTextureType.CameraTarget);
                _blitPass.Execute(context);
            }

            _command.Clear();
            RenderTextureManager.ReleaseAllTempRT(_command);
            context.ExecuteCommandBuffer(_command);

            OnCameraRenderingEnd(context,camera);
        }

        private DrawingSettings CreateDrawSettings(Camera camera){
            var sortingSetting = new SortingSettings(camera);
            var drawSetting = new DrawingSettings(_shaderTag,sortingSetting);
            
            //enable PerObjectLight
            drawSetting.perObjectData |= PerObjectData.LightData;
            drawSetting.perObjectData |= PerObjectData.LightIndices;
            return drawSetting;
        }

        /// <summary>
        /// 单个摄像机渲染结束时调用
        /// </summary>
        private void OnCameraRenderingEnd(ScriptableRenderContext context,Camera camera){
         
        }

    }

}

