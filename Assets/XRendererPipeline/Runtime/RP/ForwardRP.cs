using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{


    public class ForwardRP : BaseRP
    {
        private ShaderTagId _shaderTag = new ShaderTagId("XForwardBase");
        private LightConfigurator _lightConfigurator = new LightConfigurator();
        private RenderObjectPass _opaquePass = new RenderObjectPass(false);
        private RenderObjectPass _transparentPass = new RenderObjectPass(true);
        private ShadowCasterPass _shadowCastPass = new ShadowCasterPass();
        private RenderObjectPass _shadowDebugPass = new RenderObjectPass(false,"ShadowDebug");
        private BlitPass _blitPass = new BlitPass();

        private RenderTargetIdentifier _currentColorTarget;
        private RenderTargetIdentifier _currentDepthTarget;

        public ForwardRP(XRendererPipelineAsset setting):base(setting){
        }
      
        private void ConfigRenderTarget(ScriptableRenderContext context,ref CameraRenderDescription cameraRenderDescription){
            _commandbuffer.Clear();
            if(cameraRenderDescription.requireTempRT){
                _currentColorTarget = RenderTextureManager.AcquireColorTexture(_commandbuffer,ref cameraRenderDescription);
                _currentDepthTarget = _currentColorTarget;
            }else{
                _currentColorTarget = BuiltinRenderTextureType.CameraTarget;
                _currentDepthTarget = _currentColorTarget;
            }
            _commandbuffer.SetRenderTarget(_currentColorTarget,_currentDepthTarget);
            _commandbuffer.ClearRenderTarget(true,true,cameraRenderDescription.camera.backgroundColor);
            context.ExecuteCommandBuffer(_commandbuffer);
        }

        private void ConfigShaderPerCamera(ScriptableRenderContext context,Camera camera){
            _commandbuffer.Clear();
            AntiAliasUtil.ConfigShaderPerCamera(_commandbuffer,_setting.antiAliasSetting);
            CameraUtil.ConfigShaderProperties(_commandbuffer,camera);
            context.ExecuteCommandBuffer(_commandbuffer);
        }

        private void ConfigShadowDebugParams(ScriptableRenderContext context){
            var shadowSetting = _setting.shadowSetting;
            _commandbuffer.Clear();
            ShadowUtils.ConfigShadowDebugParams(_commandbuffer,shadowSetting);
            context.ExecuteCommandBuffer(_commandbuffer);
        }

        protected override void OnPostCameraCulling(ScriptableRenderContext context, Camera camera, ref CullingResults cullingResults)
        {
            base.OnPostCameraCulling(context, camera, ref cullingResults);

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
            _commandbuffer.Clear();
            RenderTextureManager.ReleaseAllTempRT(_commandbuffer);
            context.ExecuteCommandBuffer(_commandbuffer);
        }

    }
}
