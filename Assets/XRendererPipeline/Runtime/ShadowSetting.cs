using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  SRPLearn
{
    public enum ShadowBiasType{
        CasterVertexBias,
        ReceiverPixelBias,
    }

    public enum ShadowAAType{
        None = 0,
        PCF1,
        PCF3Fast,
        PCF3,
        PCF5,
    }

    [System.Serializable]
    public class ShadowSetting 
    {
        [SerializeField]
        [Range(10,500)]
        [Tooltip("最远阴影距离")]
        private float _maxShadowDistance = 100;  

        [SerializeField]
        [Range(1,4)]
        [Tooltip("级联阴影级数")]
        private int _shadowCascadeCount = 1;

        [SerializeField]
        [Range(1,100)]
        [Tooltip("1级联阴影比重")]
        private float _cascadeRatio1 = 1;

        [SerializeField]
        [Range(1,100)]
        [Tooltip("2级联阴影比重")]
        private float _cascadeRatio2 = 0;
        [SerializeField]
        [Range(1,100)]
        [Tooltip("3级联阴影比重")]
        private float _cascadeRatio3 = 0;

        [SerializeField]
        [Range(1,100)]
        [Tooltip("4级联阴影比重")]
        private float _cascadeRatio4 = 0;

        [SerializeField]
        private ShadowBiasType _shadowBiasType = ShadowBiasType.CasterVertexBias;

        [SerializeField]
        private ShadowAAType _shadowAAType = ShadowAAType.None;

        [SerializeField]
        private bool _debugShadowResolution = false;

        public int cascadeCount{
            get{
                return _shadowCascadeCount;
            }
        }

        public Vector3 cascadeRatio{
            get{
                var total = _cascadeRatio1;
                if(_shadowCascadeCount > 1){
                    total += _cascadeRatio2;
                }
                if(_shadowCascadeCount > 2){
                    total += _cascadeRatio3;
                }
                if(_shadowCascadeCount > 3){
                    total += _cascadeRatio4;
                }
                return new Vector3(_cascadeRatio1 / total,_cascadeRatio2 / total,_cascadeRatio3 / total);
            }
        }

        public ShadowBiasType biasType{
            get{
                return _shadowBiasType;
            }
        }

        public float shadowDistance{
            get{
                return _maxShadowDistance;
            }
        }

        public ShadowAAType shadowAAType{
            get{
                return _shadowAAType;
            }
        }

        public bool isShadowResolutionDebugOn{
            get{
                return _debugShadowResolution;
            }
        }
    }  
}

