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

        private Matrix4x4[] _worldToCasadeShadowMapMatrices = new Matrix4x4[4];
        private Vector4[] _casadeCullingSpheres = new Vector4[4];

        public ShadowCasterPass(){
            _commandBuffer.name = "ShadowCaster";
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




        /// <summary>
        /// 通过ComputeDirectionalShadowMatricesAndCullingPrimitives得到的投影矩阵，其对应的x,y,z范围分别为均为(-1,1).
        /// 因此我们需要构造坐标变换矩阵，可以将世界坐标转换到ShadowMap齐次坐标空间。对应的xy范围为(0,1),z范围为(1,0)
        /// </summary>
        static Matrix4x4 GetWorldToCasadeShadowMapSpaceMatrix(Matrix4x4 proj, Matrix4x4 view,Vector4 casadeOffsetAndScale)
        {
            //检查平台是否zBuffer反转,一般情况下，z轴方向是朝屏幕内，即近小远大。但是在zBuffer反转的情况下，z轴是朝屏幕外，即近大远小。
            if (SystemInfo.usesReversedZBuffer)
            {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }
            
            Matrix4x4 worldToShadow = proj * view;
            
            // xyz = xyz * 0.5 + 0.5. 
            // 即将xy从(-1,1)映射到(0,1),z从(-1,1)或(1,-1)映射到(0,1)或(1,0)
            var textureScaleAndBias = Matrix4x4.identity;
            //x = x * 0.5 + 0.5
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;

            //y = y * 0.5 + 0.5
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;

            //z = z * 0.5 = 0.5
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;

            //再将uv映射到casadeShadowMap的空间
            var casadeOffsetAndScaleMatrix = Matrix4x4.identity;

            //x = x * casadeOffsetAndScale.z + casadeOffsetAndScale.x
            casadeOffsetAndScaleMatrix.m00 = casadeOffsetAndScale.z;
            casadeOffsetAndScaleMatrix.m03 = casadeOffsetAndScale.x;

            //y = y * casadeOffsetAndScale.w + casadeOffsetAndScale.y
            casadeOffsetAndScaleMatrix.m11 = casadeOffsetAndScale.w;
            casadeOffsetAndScaleMatrix.m13 = casadeOffsetAndScale.y;

            return casadeOffsetAndScaleMatrix * textureScaleAndBias * worldToShadow;
        }

        private void ClearAndActiveShadowMapTexture(ScriptableRenderContext context, int shadowMapResolution){
            _commandBuffer.Clear();
            //设置渲染目标
            _commandBuffer.SetRenderTarget(_shadowMapHandler.renderTargetIdentifier,_shadowMapHandler.renderTargetIdentifier);
            
            _commandBuffer.SetViewport(new Rect(0,0,shadowMapResolution,shadowMapResolution));
            //Clear贴图
            _commandBuffer.ClearRenderTarget(true,true,Color.black,1);

            context.ExecuteCommandBuffer(_commandBuffer);
        }

        private void SetupShadowCasade(ScriptableRenderContext context,Vector2 offsetInAtlas, int resolution,ref Matrix4x4 matrixView,ref Matrix4x4 matrixProj){
            _commandBuffer.Clear();
            _commandBuffer.SetViewport(new Rect(offsetInAtlas.x,offsetInAtlas.y,resolution,resolution));
            //设置view&proj矩阵
            _commandBuffer.SetViewProjectionMatrices(matrixView,matrixProj);
            context.ExecuteCommandBuffer(_commandBuffer);
        }

        public void Execute(ScriptableRenderContext context,ref ShadowCasterSetting setting){

            ref var lightData = ref setting.lightData;
            ref var cullingResults = ref setting.cullingResults;
            var shadowSetting = setting.shadowSetting;

            if(!lightData.HasMainLight()){
                //表示场景无主灯光
                Shader.SetGlobalVector(ShaderProperties.ShadowParams,new Vector4(0,0,0,0));
                return;
            }
            //false表示该灯光对场景无影响
            if(!cullingResults.GetShadowCasterBounds(lightData.mainLightIndex,out var lightBounds)){
                Shader.SetGlobalVector(ShaderProperties.ShadowParams,new Vector4(0,0,0,0));
                return;
            }
            var mainLight = lightData.mainLight;
            var lightComp = mainLight.light;



            var shadowMapResolution = GetShadowMapResolution(lightComp);
            //生成ShadowMapTexture
            _shadowMapHandler.AcquireRenderTextureIfNot(shadowMapResolution);

            var casadeRatio = setting.shadowSetting.casadeRatio;

            this.ClearAndActiveShadowMapTexture(context,shadowMapResolution);

            var casadeAtlasGridSize = Mathf.CeilToInt(Mathf.Sqrt(shadowSetting.casadeCount));
            var casadeResolution = shadowMapResolution / casadeAtlasGridSize;

            var casadeOffsetInAtlas = new Vector2(0,0);

            for(var i = 0; i < shadowSetting.casadeCount; i ++){

                var x = i % casadeAtlasGridSize;
                var y = i / casadeAtlasGridSize;

                //计算当前级别的级联阴影在Atlas上的偏移位置
                var offsetInAtlas = new Vector2(x * casadeResolution,y * casadeResolution);

                //get light matrixView,matrixProj,shadowSplitData
                cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(lightData.mainLightIndex,i,shadowSetting.casadeCount,
                casadeRatio,casadeResolution,lightComp.shadowNearPlane,out var matrixView,out var matrixProj,out var shadowSplitData);
                
                //generate ShadowDrawingSettings
                ShadowDrawingSettings shadowDrawSetting = new ShadowDrawingSettings(cullingResults,lightData.mainLightIndex);
                shadowDrawSetting.splitData = shadowSplitData;
                
                //设置Casade相关参数
                SetupShadowCasade(context,offsetInAtlas,casadeResolution,ref matrixView,ref matrixProj);
            
                //绘制阴影
                context.DrawShadows(ref shadowDrawSetting);


                //计算Casade ShadowMap空间投影矩阵和包围圆
                var casadeOffsetAndScale = new Vector4(offsetInAtlas.x,offsetInAtlas.y,casadeResolution,casadeResolution) / shadowMapResolution;
                var matrixWorldToShadowMapSpace = GetWorldToCasadeShadowMapSpaceMatrix(matrixProj,matrixView,casadeOffsetAndScale);
                _worldToCasadeShadowMapMatrices[i] = matrixWorldToShadowMapSpace;
                _casadeCullingSpheres[i] = shadowSplitData.cullingSphere;

            }

            //setup shader params
            Shader.SetGlobalMatrixArray(ShaderProperties.WorldToMainLightCasadeShadowMapSpaceMatrices,_worldToCasadeShadowMapMatrices);
            Shader.SetGlobalVectorArray(ShaderProperties.CasadeCullingSpheres,_casadeCullingSpheres);
            //将阴影的一些数据传入Shader
            Shader.SetGlobalVector(ShaderProperties.ShadowParams,new Vector4(lightComp.shadowBias,lightComp.shadowNormalBias,lightComp.shadowStrength,shadowSetting.casadeCount));
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

        public struct ShadowCasterSetting{

            public ShadowSetting shadowSetting; 
            public CullingResults cullingResults;
            public LightData lightData;
        }


        public static class ShaderProperties{

            public static readonly int MainLightMatrixWorldToShadowSpace = Shader.PropertyToID("_XMainLightMatrixWorldToShadowMap");
            
            //
            /// <summary>
            /// 类型Matrix4x4[4]，表示每级Casade从世界到贴图空间的转换矩阵
            /// </summary>
            public static readonly int WorldToMainLightCasadeShadowMapSpaceMatrices = Shader.PropertyToID("_XWorldToMainLightCasadeShadowMapSpaceMatrices");
            
            /// <summary>
            /// 类型Vector4[4],表示每级Casade的空间裁剪包围球
            /// </summary>
            public static readonly int CasadeCullingSpheres = Shader.PropertyToID("_XCasadeCullingSpheres");

            //x为depthBias,y为normalBias,z为shadowStrength,w为当前的casade shadow数量
            public static readonly int ShadowParams = Shader.PropertyToID("_ShadowParams");
            public static readonly int MainShadowMap = Shader.PropertyToID("_XMainShadowMap");

        }
    }
}
