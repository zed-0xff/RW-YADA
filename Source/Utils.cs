using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace YADA;

public static class Utils {
    public delegate object DynInvoker();

    // field:       Find.GameInfo.RealPlayTimeInteracting
    // field+NS:    YADA.ModConfig.RootDir
    // method:      StorytellerUtilityPopulation.DebugReadout()
    // field chain: Find.CameraDriver.CurrentZoom
    public static DynInvoker FastCallAny( string fqmn, bool debug = false ){
        var a = fqmn.Split(new char[] { '.', ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if( a.Count() < 1 ){
            throw new ArgumentException("don't know how to parse " + a.Count() + " arg(s)");
        }
    
        List<string> typeParts = new List<string>();
        Type t = null;
        while( t == null && a.Any() ){
            typeParts.Add(a[0]);
            a.RemoveAt(0);
            t = AccessTools.TypeByName(string.Join(".", typeParts));
        }
        if( t == null ){
            throw new ArgumentException("cannot resolve root type");
        }

        DynamicMethod dynMethod = new DynamicMethod(fqmn, typeof(object), null, restrictedSkipVisibility: true);
        ILGenerator il = dynMethod.GetILGenerator();

        Type lastValueType = t;
        object obj = t;
        while( a.Any() ){
            string chunk = a[0];
            a.RemoveAt(0);

            if( !chunk.EndsWith("()") ){
                // field
                FieldInfo fi = AccessTools.Field((obj is Type) ? (Type)obj : obj.GetType(), chunk);
                if( fi != null ){
                    if( !fi.IsStatic && (obj == null || obj is Type) )
                        throw new ArgumentException("non-static field " + fi + " on no object");

                    obj = fi.GetValue(obj);
                    lastValueType = fi.FieldType;
                    {
                        var opcode = fi.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld;
                        if( debug ) Log.Message("[d] " + opcode + " " + fi);
                        il.Emit(opcode, fi);
                    }
                    continue;
                }
                // fi == null, try as getter
                //throw new ArgumentException("no field " + chunk + " on " + (obj == null ? "Null" : obj));
                chunk = "get_" + chunk;
            }

            // method
            MethodInfo mi = AccessTools.Method((obj is Type) ? (Type)obj : obj.GetType(), chunk.Replace("()", ""));
            if( mi == null )
                throw new ArgumentException("no method " + chunk + " on " + (obj == null ? "Null" : obj));
            if( !mi.IsStatic && (obj == null || obj is Type) )
                throw new ArgumentException("non-static method " + mi + " on no object");

            {
                var opcode = mi.IsStatic ? OpCodes.Call : OpCodes.Callvirt;
                if( debug ) Log.Message("[d] " + opcode + " " + mi);
                il.Emit(opcode, mi);
            }

            lastValueType = mi.ReturnType;

            if( a.Count() == 0 ){
                // skip last real call, only emit as opcode
                break;
            }
            obj = mi.Invoke(obj, null);
        }

        if (lastValueType.IsValueType){
            il.Emit(OpCodes.Box, lastValueType); // convert int/float/... into an object
        }

        il.Emit(OpCodes.Ret);

        return (DynInvoker)dynMethod.CreateDelegate(typeof(DynInvoker));
    }

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

    public static int Disasm(MethodInfo mi, StringBuilder sb, bool showPrefix = true, int indent = 4, bool original = true){
        var instructions = original ? PatchProcessor.GetOriginalInstructions(mi) : PatchProcessor.GetCurrentInstructions(mi);
        if( showPrefix ){
            sb.AppendLineIfNotEmpty().Append(mi.FullDescription());
        }
        foreach( var op in instructions ){
            sb.AppendLineIfNotEmpty().Append(new string(' ', indent) + op);
        }
        return instructions.Count();
    }
}
