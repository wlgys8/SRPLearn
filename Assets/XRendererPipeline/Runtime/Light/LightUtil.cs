using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPLearn{
    public class LightUtil
    {
        private static int CompareLightRenderMode(LightRenderMode m1,LightRenderMode m2){
            if(m1 == m2){
                return 0;
            }
            if(m1 == LightRenderMode.ForcePixel){
                return -1;
            }
            if(m2 == LightRenderMode.ForcePixel){
                return 1;
            }
            if(m1 == LightRenderMode.Auto){
                return -1;
            }
            if(m2 == LightRenderMode.Auto){
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 按LightRenderMode、intensity对灯光重要性其排序
        /// </summary>
        public static int CompareDirectionalLight(Light l1,Light l2){
            if(l1.renderMode == l2.renderMode){
                return (int)Mathf.Sign(l2.intensity - l1.intensity);
            }
            var ret = CompareLightRenderMode(l1.renderMode,l2.renderMode);
            if(ret == 0){
                ret = (int)Mathf.Sign(l2.intensity - l1.intensity);
            }
            return ret;
        }
    }
}
