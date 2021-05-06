using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPLearn{
    public class ShadowUtils
    {
        
        public static int GetSamplePixelSize(ShadowAAType shadowAA){
            switch(shadowAA){
                case ShadowAAType.None:
                return 1;
                case ShadowAAType.PCF1:
                return 2;
                case ShadowAAType.PCF3Fast:
                return 3;
                case ShadowAAType.PCF3:
                return 4;
                case ShadowAAType.PCF5:
                return 6;
            }
            return 1;
        }
    }
}
