using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPLearn{
    public class ShadowUtils
    {
        
        public static int GetCeilSampleKernelRadius(ShadowAAType shadowAA){
            switch(shadowAA){
                case ShadowAAType.None:
                return 1;
                case ShadowAAType.PCF1: //1 + ceil(0.5)
                return 2;
                case ShadowAAType.PCF3Fast:// 1 + ceil(1)
                return 2;
                case ShadowAAType.PCF3: //1 + ceil(1.5)
                return 3;
                case ShadowAAType.PCF5: //1 + ceil(2.5)
                return 4;
            }
            return 1;
        }

        public static bool IsPerPixelBias(ShadowBiasType type){
            switch(type){
                case ShadowBiasType.ReceiverPixelBias:
                return true;
            }
            return false;
        }

        public static bool IsPCFEnabled(ShadowAAType shadowAA){
            switch(shadowAA){
                case ShadowAAType.None:
                return false;
                case ShadowAAType.PCF1:
                case ShadowAAType.PCF3Fast:
                case ShadowAAType.PCF3:
                case ShadowAAType.PCF5:
                return true;
            }
            return false;
        }
    }
}
