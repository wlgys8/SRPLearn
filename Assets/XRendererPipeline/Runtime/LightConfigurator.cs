using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

namespace SRPLearn{
    public class LightConfigurator
    {

        private static int GetMainLightIndex(NativeArray<VisibleLight> lights){
            Light mainLight = null;
            var mainLightIndex = -1;
            var index = 0;
            foreach(var light in lights){
                if(light.lightType == LightType.Directional){
                    var lightComp = light.light;
                    if(lightComp.renderMode == LightRenderMode.ForceVertex){
                        continue;
                    }
                    if(!mainLight){
                        mainLight = lightComp;
                        mainLightIndex = index;
                    }else{
                        if(LightUtil.CompareDirectionalLight(mainLight,lightComp) > 0){
                            mainLight = lightComp;
                            mainLightIndex = index;
                        }
                    }
                }
                index ++;
            }
            return mainLightIndex;
        }

        private const int MAX_VISIBLE_OTHER_LIGHTS = 32;

        private int _mainLightIndex = -1;

        /// <summary>
        /// 记录了非平行光的位置和范围数据。
        /// </summary>
        private Vector4[] _otherLightPositionAndRanges = new Vector4[MAX_VISIBLE_OTHER_LIGHTS];
        
        /// <summary>
        /// 记录了非平行光的颜色信息
        /// </summary>
        private Vector4[] _otherLightColors = new Vector4[MAX_VISIBLE_OTHER_LIGHTS];


        private void SetPointLightData(int index,ref VisibleLight light){
            Vector4 positionAndRange = light.light.gameObject.transform.position;
            positionAndRange.w = light.range;
            _otherLightPositionAndRanges[index] = positionAndRange;
            _otherLightColors[index] = light.finalColor;
        }

        //设置非平行光源的GPU数据
        private void SetupOtherLightDatas(ref CullingResults cullingResults){
            var visibleLights = cullingResults.visibleLights;
            var lightMapIndex = cullingResults.GetLightIndexMap(Allocator.Temp);
            var otherLightIndex = 0;
            var visibleLightIndex = 0;
            foreach(var l in visibleLights){
                var visibleLight = l;
                switch(visibleLight.lightType){
                    case LightType.Directional:
                    lightMapIndex[visibleLightIndex] = -1;
                    break;
                    case LightType.Point:
                    lightMapIndex[visibleLightIndex] = otherLightIndex;
                    SetPointLightData(otherLightIndex,ref visibleLight);
                    otherLightIndex ++;
                    break;
                    default:
                    lightMapIndex[visibleLightIndex] = -1;
                    break;
                }
                visibleLightIndex ++;
                if(otherLightIndex >= MAX_VISIBLE_OTHER_LIGHTS){
                    break;
                }
            }
            for(var i = visibleLightIndex; i < lightMapIndex.Length;i ++){
                lightMapIndex[i] = -1;
            }
            cullingResults.SetLightIndexMap(lightMapIndex);
            Shader.SetGlobalVectorArray(ShaderProperties.OtherLightPositionAndRanges,_otherLightPositionAndRanges);
            Shader.SetGlobalVectorArray(ShaderProperties.OtherLightColors,_otherLightColors);
        }

        public LightData SetupShaderLightingParams(ScriptableRenderContext context, ref CullingResults cullingResults){
            var visibleLights = cullingResults.visibleLights;
            var mainLightIndex = GetMainLightIndex(visibleLights);
            if(mainLightIndex >= 0){
                var mainLight = visibleLights[mainLightIndex];
                var forward = - (Vector4)mainLight.light.gameObject.transform.forward; 
                Shader.SetGlobalVector(ShaderProperties.MainLightDirection,forward);
                Shader.SetGlobalColor(ShaderProperties.MainLightColor,mainLight.finalColor);
            }else{
                Shader.SetGlobalColor(ShaderProperties.MainLightColor,new Color(0,0,0,0));
            }
            Shader.SetGlobalColor(ShaderProperties.AmbientColor,RenderSettings.ambientLight);
            _mainLightIndex = mainLightIndex;

            SetupOtherLightDatas(ref cullingResults);
            return new LightData(){
                mainLightIndex = mainLightIndex,
                mainLight = mainLightIndex>=0 && mainLightIndex<visibleLights.Length? visibleLights[mainLightIndex]:default(VisibleLight),
            };
        }


        public class ShaderProperties{

            public static int MainLightDirection = Shader.PropertyToID("_XMainLightDirection");
            public static int MainLightColor = Shader.PropertyToID("_XMainLightColor");

            public static int AmbientColor = Shader.PropertyToID("_XAmbientColor");

            public static int OtherLightPositionAndRanges = Shader.PropertyToID("_XOtherLightPositionAndRanges");
            public static int OtherLightColors = Shader.PropertyToID("_XOtherLightColors");

        }
    }

    public struct LightData{
        public int mainLightIndex;
        public VisibleLight mainLight;

        public bool HasMainLight(){
            return mainLightIndex >= 0;
        }
    }
}
