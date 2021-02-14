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

        public bool enableSrpBatcher{
            get{
                return _srpBatcher;
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



        public XRenderPipeline(XRendererPipelineAsset setting){
            GraphicsSettings.useScriptableRenderPipelineBatching = setting.enableSrpBatcher;
            _command.name = "RenderCamera";
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
            var cullingResults = context.Cull(ref cullingParams);

            var lightData = _lightConfigurator.SetupShaderLightingParams(context,ref cullingResults);

            //投影Pass
            _shadowCastPass.Execute(context,camera,ref cullingResults,ref lightData);

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
