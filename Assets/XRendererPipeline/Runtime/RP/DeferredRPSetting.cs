using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SRPLearn{


    [System.Serializable]
    public class DeferredRPSetting
    {
        public enum FrameBufferOutputDebug{
            Off,
            Albedo,
            Normal,
            Position,
            Mentalness,
            Roughness,
            VisbleLightCount,
            Depth
        }

        public enum TileLightCullingAlgorithm{
            SphereBounds,
            SideFace
        }
        
        public const int MaxLightCountPerTile = 32;

        public const int TileBlockSize = 16;


        [SerializeField]
        private FrameBufferOutputDebug _outputDebug = FrameBufferOutputDebug.Off;

        public bool lightShadeByComputeShader = true;


        public TileLightCullingAlgorithm tileLightCullingAlgorithm = TileLightCullingAlgorithm.SphereBounds;

        public bool enableDepthSliceForLightCulling = true;


        public FrameBufferOutputDebug outputDebug{
            get{
                return _outputDebug;
            }
        }
    }
}
