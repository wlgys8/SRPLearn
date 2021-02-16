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

        protected override RenderPipeline CreatePipeline()
        {
            return new XRenderPipeline(this);
        }
    }


    public class XRenderPipeline : RenderPipeline
    {

        private ShaderTagId _shaderTag = new ShaderTagId("XForwardBase");
        private LightConfigurator _lightConfigurator = new LightConfigurator();

        private RenderObjectPass _opaquePass = new RenderObjectPass(false);
        private RenderObjectPass _transparentPass = new RenderObjectPass(true);

        private ShadowCasterPass _shadowCastPass = new ShadowCasterPass();
        private CommandBuffer _command = new CommandBuffer();


        private XRendererPipelineAsset _setting;
        public XRenderPipeline(XRendererPipelineAsset setting){
            GraphicsSettings.useScriptableRenderPipelineBatching = setting.enableSrpBatcher;
            _command.name = "RenderCamera";
            _setting = setting;
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            //遍历摄像机，进行渲染
            foreach(var camera in cameras){
                RenderPerCamera(context,camera);
            }
            //提交渲染命令
            context.Submit();
        }


        private void ClearCameraTarget(ScriptableRenderContext context,Camera camera){
            _command.Clear();
            _command.SetRenderTarget(BuiltinRenderTextureType.CameraTarget,BuiltinRenderTextureType.CameraTarget);
            _command.ClearRenderTarget(true,true,camera.backgroundColor);
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
            //投影Pass
            _shadowCastPass.Execute(context,ref casterSetting);

            //重设摄像机参数
            context.SetupCameraProperties(camera);
            //清除摄像机背景
            ClearCameraTarget(context,camera);

            //非透明物体渲染
            _opaquePass.Execute(context,camera,ref cullingResults);

            //透明物体渲染
            _transparentPass.Execute(context,camera,ref cullingResults);

        }

        private DrawingSettings CreateDrawSettings(Camera camera){
            var sortingSetting = new SortingSettings(camera);
            var drawSetting = new DrawingSettings(_shaderTag,sortingSetting);
            return drawSetting;
        }

    }

}

