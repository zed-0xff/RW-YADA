using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace zed_0xff.YADA;

public static class Utils {
    // fqmn = fully qualified method name :) like "RimWorld.Need::get_MaxLevel"
    public static MethodInfo fqmnToMethodInfo(string fqmn, out string error){
        error = null;
        var a = fqmn.Split(new char[] { '.', ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if( a.Count() < 2 ){
            error = "don't know how to parse " + a.Count() + " arg(s)";
            return null;
        }
        string methodName = a[a.Count() - 1];
        a.RemoveAt(a.Count() - 1);
        string className = String.Join(".", a);
        Type t = AccessTools.TypeByName(className);
        if( t == null ){
            error = "cannot get type for " + className;
            return null;
        }
        MethodInfo mi = AccessTools.Method(t, methodName);
        if( mi == null ){
            error = "cannot get method " + methodName;
            return null;
        }
        return mi;
    }

    public static int Disasm(MethodInfo mi, StringBuilder sb, bool showPrefix = true, int indent = 4){
        var instructions = PatchProcessor.GetOriginalInstructions(mi);
        if( showPrefix ){
            sb.AppendLineIfNotEmpty().Append("[d] " + mi);
        }
        foreach( var op in instructions ){
            sb.AppendLineIfNotEmpty().Append(new string(' ', indent) + op);
        }
        return instructions.Count();
    }
}
