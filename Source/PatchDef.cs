using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace zed_0xff.YADA;

// Field 'PatchDef.className' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

public class PatchDef : Def {
    
    public bool enabled = true; // skips this patch completely

    public string HarmonyPriority;
    public List<string> HarmonyBefore;
    public List<string> HarmonyAfter;
    public bool HarmonyDebug;
    public string HarmonyPatchCategory;

    public string className;
    public string methodName;

    public Prefix prefix;
    public Postfix postfix;

    public DebugSettingsCheckbox debugSettingsCheckbox;

    ///////////////////////////////////////////////////////////////////////////////////////////////

    public class DebugSettingsCheckbox {
        public bool defaultValue;
        public string category = "YADA";
    }

    public abstract class Anyfix {
        public string setResult = UndefinedResult;
        public List<string> opcodes;
        public List<string> arguments;

        [Unsaved(false)]
        public object setResultObj;
        [Unsaved(false)]
        public bool isResultNull;

        public void PostLoad(){
            if( setResult == UndefinedResult )
                setResult = null;
            else if( setResult == null ){
                isResultNull = true;
            }
        }
    }

    public class Prefix : Anyfix {
        public bool skipOriginal;
    }

    public class Postfix : Anyfix {
    }

    private static readonly string UndefinedResult = "19746_XXX_UNDEFINED_XXX_21831";
    private static int unnamedIdx = 0;

    [Unsaved(false)]
    public Type resultType;

    [Unsaved(false)]
    private bool autoDefName;

    public override void PostLoad() {
        if( defName == "UnnamedDef" ){
            defName = "YADA_dynamic_patch__" + unnamedIdx++ + "__" + className + "__" + methodName;
            autoDefName = true;
        }
        if( prefix != null ) prefix.PostLoad();
        if( postfix != null ) postfix.PostLoad();
        base.PostLoad();
    }

    public override IEnumerable<string> ConfigErrors() {
        foreach (string item in base.ConfigErrors()) {
            yield return item;
        }
        if (className == null) yield return "className should be set";
        if (methodName == null) yield return "methodName should be set";
        if (prefix == null && postfix == null) yield return "no prefix nor postfix is set";
        if (debugSettingsCheckbox != null && autoDefName) yield return "debugSettingsCheckbox needs explicit PatchDef.defName";

        Type t = AccessTools.TypeByName(className);
        if( t == null ){
            yield return "can't find class " + className;
            yield break;
        }
        MethodInfo m = AccessTools.Method(t, methodName);
        if( m == null ){
            yield return "can't find method " + methodName + " in " + t;
            yield break;
        }
        resultType = m.ReturnType;

        if (prefix != null){
            var err = check_anyfix(prefix);
            if( err != null) yield return err;
        }
        if (postfix != null){
            var err = check_anyfix(postfix);
            if( err != null) yield return err;
        }

        string check_anyfix(Anyfix a){
            if( a.opcodes != null && a.setResult != null ){
                return "cannot have both opcodes and setResult simultaneously";
            }
            if( a.arguments != null && a.opcodes == null ){
                return "<arguments> are only used with <opcodes>";
            }
            if( a.setResult == null ){
                a.setResultObj = null;
                a.setResult = null;
            } else {
                if( a.opcodes != null || a.arguments != null ){
                    return "either use setResult or arguments+opcodes";
                }
                if( !ParseHelper.CanParse(resultType, a.setResult) ){
                    return "can't parse \"" + a.setResult + "\" as " + resultType;
                }
                a.setResultObj = ConvertHelper.Convert(a.setResult, resultType);
            }
            return null;
        }
    }
}

