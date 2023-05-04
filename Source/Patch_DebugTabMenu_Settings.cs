using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace zed_0xff.YADA;

// hook immediate debug view settings change

//[HarmonyPatch]
//static class Patch_DebugTabMenu_Settings {
//    public static MethodBase TargetMethod()
//    {
//        Type[] nestedTypes = typeof(DebugTabMenu_Settings).GetNestedTypes(AccessTools.all);
//
//        foreach( var t in nestedTypes ){
//            foreach( var m in t.GetMethods(AccessTools.all)){
//                var param = m.GetParameters();
//                if( m.ReturnType == typeof(void)
//                        && param.Length == 0
//                        && m.Name == "<AddNode>b__0"
//                  ){
//                    return m;
//                }
//            }
//        }
//
//        // failed to find anything :(
//        return null;
//    }
//
//    static MethodInfo origMethod = AccessTools.Method(typeof(FieldInfo), nameof(FieldInfo.SetValue), new Type[]{ typeof(Object), typeof(Object) } );
//    static MethodInfo myMethod   = AccessTools.Method(typeof(Patch_DebugTabMenu_Settings), nameof(Patch_DebugTabMenu_Settings.SetValue));
//
//    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//    {
//        foreach (var code in instructions){
//            // callvirt virtual System.Void System.Reflection.FieldInfo::SetValue(System.Object obj, System.Object value)
//            if ( code.opcode == OpCodes.Callvirt && (MethodInfo)code.operand == origMethod ){
//                code.operand = myMethod;
//            }
//            yield return code;
//        }
//    }
//
//    static void SetValue(FieldInfo fi, Object obj, Object value){
//        origMethod.Invoke(fi, new object[] { obj, value } );
//        Log.Warning("[d] " + fi + "|" + obj + "=" + value );
//    }
//}
