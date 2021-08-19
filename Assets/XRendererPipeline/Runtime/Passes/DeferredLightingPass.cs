using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{
    public class DeferredLightingPass
    {

        private CommandBuffer _commandbuffer = new CommandBuffer();

        [System.NonSerialized]
        private Material _lightPassMat;
        private Mesh _fullScreenMesh;
        public DeferredLightingPass(){
            _commandbuffer.name = "DeferredLightingPass";
        }
        public void Execute(ScriptableRenderContext context){
            if(!_lightPassMat){
                _lightPassMat = new Material(Shader.Find("Hidden/SRPLearn/DeferredLightPass"));
                _lightPassMat.DisableKeyword("_RECEIVE_SHADOWS_OFF");
            }
            if(!_fullScreenMesh){
                _fullScreenMesh = Utils.CreateFullscreenMesh();
            }
            _commandbuffer.Clear();
            _commandbuffer.SetViewProjectionMatrices(Matrix4x4.identity,Matrix4x4.identity);
            _commandbuffer.DrawMesh(_fullScreenMesh,Matrix4x4.identity,_lightPassMat,0,0);
            context.ExecuteCommandBuffer(_commandbuffer);
        }

    }
}
