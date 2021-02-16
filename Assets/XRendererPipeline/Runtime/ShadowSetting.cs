using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  SRPLearn
{
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
        private float _casadeRatio1 = 1;

        [SerializeField]
        [Range(1,100)]
        [Tooltip("2级联阴影比重")]
        private float _casadeRatio2 = 0;
        [SerializeField]
        [Range(1,100)]
        [Tooltip("3级联阴影比重")]
        private float _casadeRatio3 = 0;

        [SerializeField]
        [Range(1,100)]
        [Tooltip("4级联阴影比重")]
        private float _casadeRatio4 = 0;


        public int casadeCount{
            get{
                return _shadowCascadeCount;
            }
        }

        public Vector3 casadeRatio{
            get{
                var total = _casadeRatio1;
                if(_shadowCascadeCount > 1){
                    total += _casadeRatio2;
                }
                if(_shadowCascadeCount > 2){
                    total += _casadeRatio3;
                }
                if(_shadowCascadeCount > 3){
                    total += _casadeRatio4;
                }
                return new Vector3(_casadeRatio1 / total,_casadeRatio2 / total,_casadeRatio3 / total);
            }
        }



        public float shadowDistance{
            get{
                return _maxShadowDistance;
            }
        }
    }  
}

