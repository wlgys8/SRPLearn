using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPLearn{

    [CreateAssetMenu(menuName = "SRPLearn/BuiltinAssets")]
    public class BuiltinAssets:ScriptableObject
    {
        [SerializeField]
        private Texture2D _BRDFLUT;
        [SerializeField]
        private Material _defaultMaterial;
        [SerializeField]
        private ComputeShader _deferredLightingCulling;

        [System.NonSerialized]
        private ComputeShader _deferredLightingCullingInstance;

        public Texture2D BRDFLUT{
            get{
                return _BRDFLUT;
            }
        }
        public Material defaultMaterial{
            get{
                return _defaultMaterial;
            }
        }

        public ComputeShader deferredLightingCullingCS{
            get{
                if(!_deferredLightingCullingInstance){
                    _deferredLightingCullingInstance = Object.Instantiate(_deferredLightingCulling);
                }
                return _deferredLightingCullingInstance;
            }
        }
    }

}


