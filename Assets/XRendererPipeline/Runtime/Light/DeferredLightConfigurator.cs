using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Mathematics;

namespace SRPLearn{
    public class DeferredLightConfigurator
    {
        public class PointLightComparer : IComparer<VisibleLight>
        {
            public int Compare(VisibleLight x, VisibleLight y)
            {
                var r1 = x.screenRect;
                var r2 = y.screenRect;
                return (int)Mathf.Sign(r2.width * r2.height - r1.width * r1.height);
            }
        }

        private PointLightComparer _pointLightComparer = new PointLightComparer();

        private const int MAX_LIGHT_COUNT = 1024;

        //ComputeBufferMode.Dynamic在Windows上异常,Dont know why
        private ComputeBuffer _lightPositionAndRangeBuffer = new ComputeBuffer(MAX_LIGHT_COUNT,sizeof(float)*4); //,ComputeBufferType.Default,ComputeBufferMode.Dynamic);
        private ComputeBuffer _lightColorsBuffer = new ComputeBuffer(MAX_LIGHT_COUNT,sizeof(float) * 4); //,ComputeBufferType.Default,ComputeBufferMode.Dynamic);
        private NativeArray<float4> _lightPositionAndRangeArray = new NativeArray<float4>(MAX_LIGHT_COUNT,Allocator.Persistent,NativeArrayOptions.UninitializedMemory);
        private NativeArray<float4> _lightColorArray = new NativeArray<float4>(MAX_LIGHT_COUNT,Allocator.Persistent,NativeArrayOptions.UninitializedMemory);
        private int _otherLightCount = 0;
        private bool _disposed = false;

        public DeferredLightConfigurator(){
            Shader.SetGlobalBuffer(ShaderConstants.OtherLightPositionAndRanges,_lightPositionAndRangeBuffer);
            Shader.SetGlobalBuffer(ShaderConstants.OtherLightColors,_lightColorsBuffer);
        }
        
        public LightData Prepare(ref CullingResults cullingResults){
            AssertNotDisposed();
            var visibleLights = cullingResults.visibleLights;

            NativeList<VisibleLight> pointLights = new NativeList<VisibleLight>(visibleLights.Length,Allocator.Temp);
            var lightIndex = 0;
            var pointLightIndex = 0;
            var mainLightIndex = -1;

            foreach(var l in visibleLights){
                switch(l.lightType){
                    case LightType.Directional:
                    if(mainLightIndex < 0){
                        mainLightIndex = lightIndex;
                    }else{
                        if(LightUtil.CompareDirectionalLight(l.light,visibleLights[mainLightIndex].light) < 0){
                            mainLightIndex = lightIndex;
                        }
                    }
                    //平行光
                    break;
                    case LightType.Point:
                    //点光源
                    pointLights.Add(l);
                    pointLightIndex ++;
                    break;
                }
                if(pointLightIndex >= MAX_LIGHT_COUNT){
                    break;
                }
                lightIndex ++;
            }
            // pointLights.Sort(_pointLightComparer);
            int lightCount = pointLights.Length;
            for(var i = 0; i < lightCount; i ++){
                var l = pointLights[i];
                var pos = l.light.transform.position;
                _lightPositionAndRangeArray[i] = new float4(pos.x,pos.y,pos.z,l.range);
                var finalColor = l.finalColor;
                _lightColorArray[i] = new float4(finalColor.r,finalColor.g,finalColor.b,finalColor.a);
            }
            _otherLightCount = lightCount;
            _lightPositionAndRangeBuffer.SetData(_lightPositionAndRangeArray,0,0,lightCount);
            _lightColorsBuffer.SetData(_lightColorArray,0,0,lightCount);
            Shader.SetGlobalInt(ShaderConstants.OtherLightCount,lightCount);
            if(mainLightIndex >= 0){
                var main = visibleLights[mainLightIndex];
                Shader.SetGlobalVector(ShaderConstants.MainLightDirection,- main.light.transform.forward);
                Shader.SetGlobalColor(ShaderConstants.MainLightColor,main.finalColor);
            }else{
                Shader.SetGlobalColor(ShaderConstants.MainLightColor,Vector4.zero);
            }

            pointLights.Dispose();

            return new LightData(){
                mainLight = mainLightIndex >=0 ? visibleLights[mainLightIndex]:default,
                mainLightIndex = mainLightIndex
            };
        }

        public int otherLightCount{
            get{
                return _otherLightCount;
            }
        }

        public void Dispose(){
            _disposed = true;
            _lightColorsBuffer.Dispose();
            _lightPositionAndRangeBuffer.Dispose();
            _lightColorArray.Dispose();
            _lightPositionAndRangeArray.Dispose();
        }

        private void AssertNotDisposed(){
            if(_disposed){
                throw new System.ObjectDisposedException(this.GetType().Name);
            }
        }

        public class ShaderConstants{   
            
            public static int MainLightDirection = Shader.PropertyToID("_XMainLightDirection");
            public static int MainLightColor = Shader.PropertyToID("_XMainLightColor");
            public static readonly int OtherLightPositionAndRanges = Shader.PropertyToID("_DeferredOtherLightPositionAndRanges");
            public static int OtherLightColors = Shader.PropertyToID("_DeferredOtherLightColors");
            public static int OtherLightCount = Shader.PropertyToID("_DeferredOtherLightCount");
        }
    }
}
