using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

namespace SRPLearn{


    [CreateAssetMenu(menuName = "SRPLearn/XRendererPipelineAsset")]
    public class XRendererPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new XRenderPipeline();
        }
    }


    public class XRenderPipeline : RenderPipeline
    {

        private ShaderTagId _shaderTag = new ShaderTagId("ForwardBase");
        

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            //遍历摄像机，进行渲染
            foreach(var camera in cameras){
                RenderPerCamera(context,camera);
            }
            //提交渲染命令
            context.Submit();
        }



        private void RenderPerCamera(ScriptableRenderContext context,Camera camera){
            //设置摄像机参数
            context.SetupCameraProperties(camera);
            //对场景进行裁剪
            camera.TryGetCullingParameters( out var cullingParams);
            var cullingResults = context.Cull(ref cullingParams);
            var drawSetting = CreateDrawSettings(camera);
            var filterSetting = new FilteringSettings(RenderQueueRange.all);
            //绘制物体
            context.DrawRenderers(cullingResults,ref drawSetting,ref filterSetting);
        }

        private DrawingSettings CreateDrawSettings(Camera camera){
            var sortingSetting = new SortingSettings(camera);
            var drawSetting = new DrawingSettings(_shaderTag,sortingSetting);
            return drawSetting;
        }

    }

}
