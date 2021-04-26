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

        private Matrix4x4[] _worldToCascadeShadowMapMatrices = new Matrix4x4[4];
        private Vector4[] _cascadeCullingSpheres = new Vector4[4];

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
        static Matrix4x4 GetWorldToCascadeShadowMapSpaceMatrix(Matrix4x4 proj, Matrix4x4 view,Vector4 cascadeOffsetAndScale)
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

            //再将uv映射到cascadeShadowMap的空间
            var cascadeOffsetAndScaleMatrix = Matrix4x4.identity;

            //x = x * cascadeOffsetAndScale.z + cascadeOffsetAndScale.x
            cascadeOffsetAndScaleMatrix.m00 = cascadeOffsetAndScale.z;
            cascadeOffsetAndScaleMatrix.m03 = cascadeOffsetAndScale.x;

            //y = y * cascadeOffsetAndScale.w + cascadeOffsetAndScale.y
            cascadeOffsetAndScaleMatrix.m11 = cascadeOffsetAndScale.w;
            cascadeOffsetAndScaleMatrix.m13 = cascadeOffsetAndScale.y;

            return cascadeOffsetAndScaleMatrix * textureScaleAndBias * worldToShadow;
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

        private void SetupShadowCascade(ScriptableRenderContext context,Vector2 offsetInAtlas, int resolution,ref Matrix4x4 matrixView,ref Matrix4x4 matrixProj){
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

            var cascadeRatio = setting.shadowSetting.cascadeRatio;

            this.ClearAndActiveShadowMapTexture(context,shadowMapResolution);

            var cascadeAtlasGridSize = Mathf.CeilToInt(Mathf.Sqrt(shadowSetting.cascadeCount));
            var cascadeResolution = shadowMapResolution / cascadeAtlasGridSize;

            var cascadeOffsetInAtlas = new Vector2(0,0);

            for(var i = 0; i < shadowSetting.cascadeCount; i ++){

                var x = i % cascadeAtlasGridSize;
                var y = i / cascadeAtlasGridSize;

                //计算当前级别的级联阴影在Atlas上的偏移位置
                var offsetInAtlas = new Vector2(x * cascadeResolution,y * cascadeResolution);

                //get light matrixView,matrixProj,shadowSplitData
                cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(lightData.mainLightIndex,i,shadowSetting.cascadeCount,
                cascadeRatio,cascadeResolution,lightComp.shadowNearPlane,out var matrixView,out var matrixProj,out var shadowSplitData);
                
                //generate ShadowDrawingSettings
                ShadowDrawingSettings shadowDrawSetting = new ShadowDrawingSettings(cullingResults,lightData.mainLightIndex);
                shadowDrawSetting.splitData = shadowSplitData;
                
                //设置Cascade相关参数
                SetupShadowCascade(context,offsetInAtlas,cascadeResolution,ref matrixView,ref matrixProj);
            
                //绘制阴影
                context.DrawShadows(ref shadowDrawSetting);


                //计算Cascade ShadowMap空间投影矩阵和包围圆
                var cascadeOffsetAndScale = new Vector4(offsetInAtlas.x,offsetInAtlas.y,cascadeResolution,cascadeResolution) / shadowMapResolution;
                var matrixWorldToShadowMapSpace = GetWorldToCascadeShadowMapSpaceMatrix(matrixProj,matrixView,cascadeOffsetAndScale);
                _worldToCascadeShadowMapMatrices[i] = matrixWorldToShadowMapSpace;
                _cascadeCullingSpheres[i] = shadowSplitData.cullingSphere;

            }

            //setup shader params
            Shader.SetGlobalMatrixArray(ShaderProperties.WorldToMainLightCascadeShadowMapSpaceMatrices,_worldToCascadeShadowMapMatrices);
            Shader.SetGlobalVectorArray(ShaderProperties.CascadeCullingSpheres,_cascadeCullingSpheres);
            //将阴影的一些数据传入Shader
            Shader.SetGlobalVector(ShaderProperties.ShadowParams,new Vector4(lightComp.shadowBias,lightComp.shadowNormalBias,lightComp.shadowStrength,shadowSetting.cascadeCount));
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
            /// 类型Matrix4x4[4]，表示每级Cascade从世界到贴图空间的转换矩阵
            /// </summary>
            public static readonly int WorldToMainLightCascadeShadowMapSpaceMatrices = Shader.PropertyToID("_XWorldToMainLightCascadeShadowMapSpaceMatrices");
            
            /// <summary>
            /// 类型Vector4[4],表示每级Cascade的空间裁剪包围球
            /// </summary>
            public static readonly int CascadeCullingSpheres = Shader.PropertyToID("_XCascadeCullingSpheres");

            //x为depthBias,y为normalBias,z为shadowStrength,w为当前的cascade shadow数量
            public static readonly int ShadowParams = Shader.PropertyToID("_ShadowParams");
            public static readonly int MainShadowMap = Shader.PropertyToID("_XMainShadowMap");

        }
    }
}
