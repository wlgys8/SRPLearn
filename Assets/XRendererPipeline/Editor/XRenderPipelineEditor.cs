using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SRPLearn.Editor{
    public class XRenderPipelineEditor
    {
        [InitializeOnLoadMethod]
        private static void Startup(){
        }

        private static Color[] cascadeColors = {
            new Color(1,0,0,1),
            new Color(0,1,0,1),
            new Color(0,0,1,1),
            new Color(1,1,0,1)
        };


        [DrawGizmo(GizmoType.NonSelected|GizmoType.Selected|GizmoType.InSelectionHierarchy)]
        public static void DrawCameraCSM(Camera camera,GizmoType gizmoType){
            if(!ShadowDebug.drawCascadeCullingSphere){
                return;
            }
            var color = Gizmos.color;
            var cameraId = camera.GetInstanceID();
            var cascadeCount = ShadowDebug.GetCascadeCount(cameraId);
            if(cascadeCount == 0){
                return;
            }
            for(var i = cascadeCount - 1; i >=0 ; i --){
                var cullingSphere = ShadowDebug.GetCascadeCullingSphere(cameraId,i);
                var cascadeColor = cascadeColors[i];
                cascadeColor.a = 0.5f;
                Gizmos.color = cascadeColor;
                Gizmos.DrawSphere((Vector3)cullingSphere,cullingSphere.w);
            }
            var originalMatrix = Gizmos.matrix;
            for(var i = cascadeCount - 1; i >=0 ; i --){
                var matrixView = ShadowDebug.GetViewMatrix(cameraId,i);
                Gizmos.matrix = matrixView.inverse;
                var cascadeColor = cascadeColors[i];
                Gizmos.color = cascadeColor;
                var matrixProj = ShadowDebug.GetProjMatrix(cameraId,i);
                var near = (matrixProj.m23 + 1) /matrixProj.m22;
                var far = (matrixProj.m23 - 1) /matrixProj.m22;
                var width = 2  / matrixProj.m00;
                var height = 2 / matrixProj.m11;
                Gizmos.DrawWireCube(new Vector3(0,0, -(near + far) / 2),new Vector3(width,height,far-near));
            }
            Gizmos.matrix = originalMatrix;
            Gizmos.color = color;
        }
    }
}
