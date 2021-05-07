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
                case ShadowAAType.PCF5:
                return true;
            }
            return false;
        }
    }
}
