using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SRPLearn;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class SwitchRenderPipeline : MonoBehaviour
{
    public RenderPath renderPath;

    void Awake()
    {
        if (GraphicsSettings.renderPipelineAsset is XRendererPipelineAsset xRendererPipelineAsset)
        {
            xRendererPipelineAsset.renderPath = renderPath;
        }
    }
}
