using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{
    public class BlitPass
    {
        private static readonly int _BlitTex = Shader.PropertyToID("_BlitTex");
        private Material _blitMaterial;
        private RenderTargetIdentifier _source;
        private RenderTargetIdentifier _target;
        private CommandBuffer _command;

        public BlitPass(){
             _command = new CommandBuffer();
             _command.name = "Blit";
        }

        public void Config(RenderTargetIdentifier source,RenderTargetIdentifier target){
            _source = source;
            _target = target;
        }

        public void Execute(ScriptableRenderContext context){
            if(!_blitMaterial){
                _blitMaterial = new Material(Shader.Find("Hidden/SRPLearn/Blit"));
            }
            _command.Clear();
            _command.SetGlobalTexture(_BlitTex,_source);
            _command.Blit(_source,_target,_blitMaterial);
            context.ExecuteCommandBuffer(_command);
        }
    }
}
