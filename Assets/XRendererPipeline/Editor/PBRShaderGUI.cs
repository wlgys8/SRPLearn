using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SRPLearn.Editor
{
    public class PBRShaderGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);
            var material = materialEditor.target as Material;
            var IBLSpec = material.GetTexture("_IBLSpec") as Cubemap;
            if (!IBLSpec)
            {
                material.DisableKeyword("_PBR_IBL_SPEC");
                material.DisableKeyword("_PBR_IBL_DIFF");
            }
            else
            {
                material.EnableKeyword("_PBR_IBL_SPEC");
                material.EnableKeyword("_PBR_IBL_DIFF");
                material.SetInt("_IBLSpecMaxMip", IBLSpec.mipmapCount);
            }

            var bumpMap = material.GetTexture("_BumpMap");
            if (bumpMap)
            {
                material.EnableKeyword("ENABLE_NORMAL_MAP");
            }
            else
            {
                material.DisableKeyword("ENABLE_NORMAL_MAP");
            }
        }
    }
}
