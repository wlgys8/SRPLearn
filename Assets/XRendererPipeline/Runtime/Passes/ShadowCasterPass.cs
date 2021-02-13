using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn
{
    
    public class ShadowCasterPass
    {


        private CommandBuffer _commandBuffer = new CommandBuffer();

        private ShadowMapTextureHandler _shadowMapHandler = new ShadowMapTextureHandler();

        public ShadowCasterPass(){
            _commandBuffer.name = "ShadowCaster";
        }



        private void ConfigShaderParams(ref LightData lightData){
            var mainLight = lightData.mainLight;
            var lightDirection = mainLight.light.gameObject.transform.forward;
        }

        private static int GetShadowMapResolution(Light light){
            switch(light.shadowResolution){
                case LightShadowResolution.VeryHigh:
                return 2048;
                case LightShadowResolution.High:
                return 1024;
                case LightShadowResolution.Medium:
                return 512;
                case LightShadowResolution.Low:
                return 256;
            }
            return 256;
        }

        private void SetupShadowCasterView(ScriptableRenderContext context,int shadowMapResolution,ref Matrix4x4 matrixView,ref Matrix4x4 matrixProj){
            _commandBuffer.Clear();
            _commandBuffer.SetViewport(new Rect(0,0,shadowMapResolution,shadowMapResolution));
            //设置view&proj矩阵
            _commandBuffer.SetViewProjectionMatrices(matrixView,matrixProj);
            //设置渲染目标
            _commandBuffer.SetRenderTarget(_shadowMapHandler.renderTargetIdentifier,_shadowMapHandler.renderTargetIdentifier);
            //Clear贴图
            _commandBuffer.ClearRenderTarget(true,true,Color.black,1);
            context.ExecuteCommandBuffer(_commandBuffer);
        }

        /// <summary>
        /// 通过ComputeDirectionalShadowMatricesAndCullingPrimitives得到的投影矩阵，其对应的x,y,z范围分别为均为(-1,1).
        /// 因此我们需要构造坐标变换矩阵，可以将世界坐标转换到ShadowMap齐次坐标空间。对应的xy范围为(0,1),z范围为(1,0)
        /// </summary>
        static Matrix4x4 GetWorldToShadowMapSpaceMatrix(Matrix4x4 proj, Matrix4x4 view)
        {
            //检查平台是否zBuffer反转,一般情况下，z轴方向是朝屏幕内，即近小远大。但是在zBuffer反转的情况下，z轴是朝屏幕外，即近大远小。
            if (SystemInfo.usesReversedZBuffer)
            {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }

            // uv_depth = xyz * 0.5 + 0.5. 
            // 即将xy从(-1,1)映射到(0,1),z从(-1,1)或(1,-1)映射到(0,1)或(1,0)
            Matrix4x4 worldToShadow = proj * view;
            var textureScaleAndBias = Matrix4x4.identity;
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;

            return textureScaleAndBias * worldToShadow;
        }
        public void Execute(ScriptableRenderContext context,Camera camera,ref CullingResults cullingResults,ref LightData lightData){
            //false表示该灯光对场景无影响
            if(!cullingResults.GetShadowCasterBounds(lightData.mainLightIndex,out var lightBounds)){
                return;
            }

            var mainLight = lightData.mainLight;
            var lightComp = mainLight.light;

            var shadowMapResolution = GetShadowMapResolution(lightComp);

            //get light matrixView,matrixProj,shadowSplitData
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(lightData.mainLightIndex,0,1,
            new Vector3(1,0,0),shadowMapResolution,lightComp.shadowNearPlane,out var matrixView,out var matrixProj,out var shadowSplitData);
            var matrixWorldToShadowMapSpace = GetWorldToShadowMapSpaceMatrix(matrixProj,matrixView);

            //generate ShadowDrawingSettings
            ShadowDrawingSettings shadowDrawSetting = new ShadowDrawingSettings(cullingResults,lightData.mainLightIndex);
            shadowDrawSetting.splitData = shadowSplitData;
            
            //setup shader params
            Shader.SetGlobalMatrix(ShaderProperties.MainLightMatrixWorldToShadowSpace,matrixWorldToShadowMapSpace);
            Shader.SetGlobalVector(ShaderProperties.ShadowParams,new Vector4(lightComp.shadowBias,lightComp.shadowNormalBias,lightComp.shadowStrength,0));

            //生成ShadowMapTexture
            _shadowMapHandler.AcquireRenderTextureIfNot(shadowMapResolution);

            //设置投影相关参数
            SetupShadowCasterView(context,shadowMapResolution,ref matrixView,ref matrixProj);
         
            //绘制阴影
            context.DrawShadows(ref shadowDrawSetting);
        }

        public class ShadowMapTextureHandler{
            private RenderTargetIdentifier _renderTargetIdentifier = "_XMainShadowMap";
            private int _shadowmapId = Shader.PropertyToID("_XMainShadowMap");
            private RenderTexture _shadowmapTexture;    

            public RenderTargetIdentifier renderTargetIdentifier{
                get{
                    return _renderTargetIdentifier;
                }
            }


            public void AcquireRenderTextureIfNot(int resolution){
                if(_shadowmapTexture && _shadowmapTexture.width != resolution){
                    //resolution changed
                    RenderTexture.ReleaseTemporary(_shadowmapTexture);
                    _shadowmapTexture = null;
                }

                if(!_shadowmapTexture){
                    _shadowmapTexture = RenderTexture.GetTemporary(resolution,resolution,16,RenderTextureFormat.Shadowmap);
                    Shader.SetGlobalTexture(ShaderProperties.MainShadowMap,_shadowmapTexture);
                    _renderTargetIdentifier = new RenderTargetIdentifier(_shadowmapTexture);
                }
            }

        }


        public static class ShaderProperties{

            public static readonly int MainLightMatrixWorldToShadowSpace = Shader.PropertyToID("_XMainLightMatrixWorldToShadowMap");

            //x为depthBias,y为normalBias,z为shadowStrength
            public static readonly int ShadowParams = Shader.PropertyToID("_ShadowParams");
            public static readonly int MainShadowMap = Shader.PropertyToID("_XMainShadowMap");

        }
    }
}
