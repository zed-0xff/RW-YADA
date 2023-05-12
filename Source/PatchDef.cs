using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace zed_0xff.YADA;

// Field 'PatchDef.className' is never assigned to / never used
#pragma warning disable CS0649, CS0169

public class PatchDef : Def {

    public enum PatchType { Undefined, Prefix, Postfix }
    public enum ActionType { None, LogValue, ReturnConst }

    public class Prefix {
        public bool skipOriginal;
        public string setResult;

        [Unsaved(false)]
        public object setResultObj;
    }

    public class Postfix {
        public string setResult;

        [Unsaved(false)]
        public object setResultObj;
    }

    public string className;
    public string methodName;

    public Prefix prefix;
    public Postfix postfix;

    private static int unnamedIdx = 0;

    [Unsaved(false)]
    public Type resultType;

    public override void PostLoad() {
        if( defName == "UnnamedDef" ){
            defName = "YADA_dynamic_patch__" + unnamedIdx++ + "__" + className + "__" + methodName;
        }
        base.PostLoad();
    }

    public override IEnumerable<string> ConfigErrors() {
        foreach (string item in base.ConfigErrors()) {
            yield return item;
        }
        if (className == null) yield return "className should be set";
        if (methodName == null) yield return "methodName should be set";
        if (prefix == null && postfix == null) yield return "no prefix nor postfix is set";

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
            if( prefix.setResult != null ){
                if( !ParseHelper.CanParse(resultType, prefix.setResult) ){
                    yield return "can't parse \"" + prefix.setResult + "\" as " + resultType;
                    yield break;
                }
                prefix.setResultObj = ConvertHelper.Convert(prefix.setResult, resultType);
            }
        }
        if (postfix != null){
            if( postfix.setResult != null ){
                if( !ParseHelper.CanParse(resultType, postfix.setResult) ){
                    yield return "can't parse \"" + postfix.setResult + "\" as " + resultType;
                    yield break;
                }
                postfix.setResultObj = ConvertHelper.Convert(postfix.setResult, resultType);
            }
        }
    }
}

#pragma warning restore CS0649, CS0169
