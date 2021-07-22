using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SRPLearn{
    public class ShadowDebug
    {
        private class CameraShadowDebugData{

            public int cascadeCount = 0;
            public Vector4[] cullingSpheres = new Vector4[4];
            public float shadowDistance = 0;
            public Matrix4x4[] matrixView = new Matrix4x4[4];
            public Matrix4x4[] matrixProj = new Matrix4x4[4];
        }


        private static Dictionary<int,CameraShadowDebugData> _cameraShadowDebugDatas = new Dictionary<int, CameraShadowDebugData>();

        private static bool _drawCascadeCullingSphere;

        public static void Setup(ShadowSetting setting){
            _drawCascadeCullingSphere = setting.drawCascadeCullingSphere;
        }

        public static bool drawCascadeCullingSphere{
            get{
                return _drawCascadeCullingSphere;
            }
        }

        public static IReadOnlyCollection<int> cameraIds{
            get{
                return _cameraShadowDebugDatas.Keys;
            }
        }

        private static CameraShadowDebugData GetCameraShadowDebugData(int cameraId){
            if(!_cameraShadowDebugDatas.ContainsKey(cameraId)){
                _cameraShadowDebugDatas.Add(cameraId,new CameraShadowDebugData());
            }
            return _cameraShadowDebugDatas[cameraId];
        }

        public static void SetShadowDistance(int cameraId,float shadowDistance){
            GetCameraShadowDebugData(cameraId).shadowDistance = shadowDistance;
        }

        public static void SetCSMCullingSpheres(int cameraId,int cascadeIndex,Vector4 cullingSphere){
            var data = GetCameraShadowDebugData(cameraId);
            data.cullingSpheres[cascadeIndex] = cullingSphere;
        }

        public static void SetCSMViewProjMatrix(int cameraId,int cascadeIndex,Matrix4x4 view,Matrix4x4 proj){
            var data = GetCameraShadowDebugData(cameraId);
            data.matrixView[cascadeIndex] = view;
            data.matrixProj[cascadeIndex] = proj;
        }

        public static int GetCascadeCount(int cameraId){
            return GetCameraShadowDebugData(cameraId).cascadeCount;
        }

        public static void SetCascadeCount(int cameraId,int count){
            GetCameraShadowDebugData(cameraId).cascadeCount = count;
        }

        public static Vector4 GetCascadeCullingSphere(int cameraId,int cascadeIndex){
            return GetCameraShadowDebugData(cameraId).cullingSpheres[cascadeIndex];
        }

        public static float GetShadowDistance(int cameraId){
            return GetCameraShadowDebugData(cameraId).shadowDistance;
        }

        public static Matrix4x4 GetViewMatrix(int cameraId,int cascadeIndex){
            return GetCameraShadowDebugData(cameraId).matrixView[cascadeIndex];
        }

        public static Matrix4x4 GetProjMatrix(int cameraId,int cascadeIndex){
            return GetCameraShadowDebugData(cameraId).matrixProj[cascadeIndex];
        }
    }
}
