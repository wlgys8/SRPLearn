using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{
    public static class Utils
    {

        
        public static void SetGlobalShaderKeyword(CommandBuffer commandBuffer,string keyword,bool enable){
            if(enable){
                commandBuffer.EnableShaderKeyword(keyword);
            }else{
                commandBuffer.DisableShaderKeyword(keyword);
            }
        }


        public static CameraRenderDescription GetCameraRenderDescription(Camera camera,XRendererPipelineAsset asset){
            var aa = asset.antiAliasSetting;
            CameraRenderDescription des = new CameraRenderDescription(camera);
            bool msaaEnable = aa.antiAliasType == AAType.MSAA && camera.allowMSAA && asset.renderPath == RenderPath.Forward;
            des.requireTempRT = msaaEnable || aa.isFXAAOn;
            if(msaaEnable){
                des.msaaLevel = aa.msaaLevel;
            }
            return des;
        }

        public static Mesh CreateFullscreenMesh()
        {
            Vector3[] positions =
            {
                new Vector3(-1.0f,  -1.0f, 0.0f),
                new Vector3(1.0f, -1.0f, 0.0f),
                new Vector3(-1.0f,  1.0f, 0.0f),
                new Vector3(1.0f, 1.0f, 0.0f),
            };
            Vector2[] uvs = {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),
            };

            int[] indices = { 0, 2, 1,1,2,3};

            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt16;
            mesh.vertices = positions;
            mesh.triangles = indices;
            mesh.uv = uvs;
            return mesh;
        }


    }
}
