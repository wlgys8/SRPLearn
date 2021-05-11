using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SRPLearn{

    public enum AAType{
        Off,
        MSAA,
    }
    
    [System.Serializable]
    public class AntiAliasSetting
    {
        public AAType antiAliasType;

        [Range(2,8)]
        public int msaaLevel = 2;
    }
}
