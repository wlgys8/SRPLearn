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
            EditorGUI.BeginChangeCheck();
            var sssEnable = EditorGUILayout.Toggle("Subsurface Scattering", material.IsKeywordEnabled("ENABLE_SSS"));
            if (EditorGUI.EndChangeCheck())
            {
                if (sssEnable)
                {
                    material.EnableKeyword("ENABLE_SSS");
                }
                else
                {
                    material.DisableKeyword("ENABLE_SSS");
                }
            }
            if (sssEnable)
            {
                var color = material.GetColor("_SSS_SimpleWrap");
                EditorGUI.BeginChangeCheck();
                color = EditorGUILayout.ColorField("Subsurface Scattering Wrap", color);
                if (EditorGUI.EndChangeCheck())
                {
                    material.SetColor("_SSS_SimpleWrap", color);
                }
            }

        }
    }
}
