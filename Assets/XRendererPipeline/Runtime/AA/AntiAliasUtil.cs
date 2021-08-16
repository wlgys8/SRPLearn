using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SRPLearn{


    internal static class AntiAliasShaderKeywords{
        public static readonly string FXAA_V1 = "FXAA_V1";
        public static readonly string FXAA_QUALITY = "FXAA_QUALITY";
        public static readonly string FXAA_CONSOLE = "FXAA_CONSOLE";
        public static readonly string FXAA_DEBUG_EDGE = "FXAA_DEBUG_EDGE";
        public static readonly string FXAA_DEBUG_CULL_PASS = "FXAA_DEBUG_CULL_PASS";
    }

    internal static class  AntiAliasShaderVars
    {
        public static readonly int FXAA_PARAMS = Shader.PropertyToID("FXAA_PARAMS");
        public static readonly int FXAA_QUALITY_SUBPIX = Shader.PropertyToID("FXAA_QUALITY_SUBPIX");
    }
    
    internal class AntiAliasUtil
    {

        public static void ConfigShaderPerCamera(CommandBuffer command, AntiAliasSetting aliasSetting){
            ConfigShaderKeywords(command,aliasSetting);
            ConfigShaderParams(command,aliasSetting);
        }

        private static void ConfigShaderKeywords(CommandBuffer command, AntiAliasSetting aliasSetting){
            Utils.SetGlobalShaderKeyword(command,AntiAliasShaderKeywords.FXAA_V1,aliasSetting.antiAliasType == AAType.FXAAV1);
            Utils.SetGlobalShaderKeyword(command,AntiAliasShaderKeywords.FXAA_QUALITY,aliasSetting.antiAliasType == AAType.FXAAQuality);
            Utils.SetGlobalShaderKeyword(command,AntiAliasShaderKeywords.FXAA_CONSOLE,aliasSetting.antiAliasType == AAType.FXAAConsole);
            Utils.SetGlobalShaderKeyword(command,AntiAliasShaderKeywords.FXAA_DEBUG_EDGE,aliasSetting.fXAADebug.debugEdge);
            Utils.SetGlobalShaderKeyword(command,AntiAliasShaderKeywords.FXAA_DEBUG_CULL_PASS,aliasSetting.fXAADebug.cullPassThrough);
            
        }

        private static void ConfigShaderParams(CommandBuffer command,AntiAliasSetting aliasSetting){
            var fxaaConfig = aliasSetting.fXAAConfig;
            if(aliasSetting.antiAliasType == AAType.FXAAQuality || aliasSetting.antiAliasType == AAType.FXAAConsole || aliasSetting.antiAliasType == AAType.FXAAV1){
                command.SetGlobalVector(AntiAliasShaderVars.FXAA_PARAMS,new Vector4(fxaaConfig.absoluteLumaThreshold,fxaaConfig.relativeLumaThreshold,fxaaConfig.consoleCharpness));
            }
            if(aliasSetting.antiAliasType == AAType.FXAAV1){
                command.SetGlobalVector(AntiAliasShaderVars.FXAA_QUALITY_SUBPIX,new Vector4(fxaaConfig.qualitySubPixTrim,fxaaConfig.qualitySubPixCap,0,0));
            }
        }

    }
}
