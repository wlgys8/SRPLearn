using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

namespace SRPLearn{

    public enum RenderPath{
        Forward,
        Deferred
    }

    [CreateAssetMenu(menuName = "SRPLearn/XRendererPipelineAsset")]
    public class XRendererPipelineAsset : RenderPipelineAsset
    {
        
        [SerializeField]
        private bool _srpBatcher = true;

        [SerializeField]
        private ShadowSetting _shadowSetting = new ShadowSetting();

        [SerializeField]
        private AntiAliasSetting _antiAlias = new AntiAliasSetting();

        [SerializeField]
        private BuiltinAssets _builtinAssets;

        [SerializeField]
        private RenderPath _renderPath = RenderPath.Forward;

        [SerializeField]
        private DeferredRPSetting _deferredSetting = new DeferredRPSetting();

        protected override void OnValidate()
        {
            base.OnValidate();
            #if UNITY_EDITOR
            if(_builtinAssets.defaultMaterial){
                _builtinAssets.defaultMaterial.hideFlags = HideFlags.NotEditable;
            }
            #endif
        }

        public DeferredRPSetting deferredRPSetting{
            get{
                return _deferredSetting;
            }
        }

        public RenderPath renderPath{
            get{
                return _renderPath;
            }
        }

        public BuiltinAssets builtinAssets{
            get{
                return _builtinAssets;
            }
        }

        public bool enableSrpBatcher{
            get{
                return _srpBatcher;
            }
        }

        public ShadowSetting shadowSetting{
            get{
                return _shadowSetting;
            }
        }

        public AntiAliasSetting antiAliasSetting{
            get{
                return _antiAlias;
            }
        }

        public override Material defaultMaterial{
            get{
                return _builtinAssets.defaultMaterial;
            }
        }

        protected override RenderPipeline CreatePipeline()
        {
            if(_renderPath == RenderPath.Forward){
                return new ForwardRP(this);
            }else if(_renderPath == RenderPath.Deferred){
                if(DeferredRP.support){
                    return new DeferredRP(this);
                }else{
                    Debug.LogWarning("Device do not support required MRT for Deferred Shading");
                    _renderPath = RenderPath.Forward;
                    return new ForwardRP(this);
                }
            }
            return null;
        }
    }


}

