using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SRPLearn{

    public enum AAType{
        Off,
        MSAA,
        FXAAV1,
        FXAAQuality,
        FXAAConsole,
    }

    [System.Serializable]
    public class FXAADebugSetting{
        public bool debugEdge;
        public bool cullPassThrough;
    }

    [System.Serializable]
    public class FXAAConfig{

        /// <summary>
        /// 当对比度大于absoluteLumaThreshold时，视为边缘。
        /// </summary>
        [Range(0.01f,0.5f)]
        public float absoluteLumaThreshold = 0.03f;

        /// <summary>
        /// 当像素对比度大于lumaMax * relativeLumaThreshold时视为边缘
        /// </summary>
        [Range(0.1f,0.5f)]
        public float relativeLumaThreshold = 0.25f;
        
        [Range(0.1f,10)]
        public float consoleCharpness = 1;

        [Range(0,0.5f)]
        public float qualitySubPixTrim = 0;

        [Range(0.75f,1f)]
        public float qualitySubPixCap = 1;


    }
    
    [System.Serializable]
    public class AntiAliasSetting
    {
        public AAType antiAliasType;

        [Range(2,8)]
        public int msaaLevel = 2;

        public FXAADebugSetting fXAADebug = new FXAADebugSetting();

        public FXAAConfig fXAAConfig = new FXAAConfig();
    }
}
