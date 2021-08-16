using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{
    public abstract class BaseRP : RenderPipeline
    {

        protected XRendererPipelineAsset _setting;

        protected CommandBuffer _commandbuffer;
        public BaseRP(XRendererPipelineAsset setting){
            GraphicsSettings.useScriptableRenderPipelineBatching = setting.enableSrpBatcher;
            _setting = setting;
            _commandbuffer = new CommandBuffer();
            _commandbuffer.name = "RP";
            Shader.SetGlobalTexture("_BRDFLUT",setting.builtinAssets.BRDFLUT);
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            OnPipelineBegin();
            ShadowDebug.Setup(_setting.shadowSetting);
            this.ConfigShaderPropertiesPipeline(context);
            CameraUtil.SortCameras(cameras);
            //遍历摄像机，进行渲染
            foreach(var camera in cameras){
                RenderPerCamera(context,camera);
            }
            //提交渲染命令
            context.Submit();
            OnPipelineEnd();
        }
        protected virtual void RenderPerCamera(ScriptableRenderContext context,Camera camera){
            //设置摄像机参数
            context.SetupCameraProperties(camera);
            //对场景进行裁剪
            camera.TryGetCullingParameters( out var cullingParams);
            cullingParams.shadowDistance = Mathf.Min(_setting.shadowSetting.shadowDistance,camera.farClipPlane - camera.nearClipPlane);
            var cullingResults = context.Cull(ref cullingParams);
            this.OnPostCameraCulling(context,camera,ref cullingResults);
        }

        protected virtual void OnPostCameraCulling(ScriptableRenderContext context,Camera camera,ref CullingResults cullingResults){
        }

        protected virtual void ConfigShaderPropertiesPipeline(ScriptableRenderContext context){
            _commandbuffer.Clear();
             if(_setting.shadowSetting.isCSMBlendEnabled){
                 _commandbuffer.EnableShaderKeyword(ShadowCasterPass.ShaderKeywords.CSMBlend);
            }else{
                _commandbuffer.DisableShaderKeyword(ShadowCasterPass.ShaderKeywords.CSMBlend);
            }
            ShadowUtils.ConfigCascadeDistances(_commandbuffer,_setting.shadowSetting);
            context.ExecuteCommandBuffer(_commandbuffer);
        }

        protected virtual void OnPipelineBegin(){}

        protected virtual void OnPipelineEnd(){}
    }
}
