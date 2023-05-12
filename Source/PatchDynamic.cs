using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace zed_0xff.YADA;

public class PatchDynamic {

    public static void PatchAll(){
        Harmony harmony = new Harmony("zed_0xff.YADA");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("yada_dyn_ass"), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("yada_dyn_mod");

        foreach( PatchDef patchDef in DefDatabase<PatchDef>.AllDefsListForReading ){
            if( patchDef.ConfigErrors().Any() ) continue;

            //Log.Warning("[d] " + patchDef + " " + patchDef.fileName + " " + patchDef.modContentPack);

            var tb = module.DefineType("YADA_Patch_" + patchDef.defName.Replace("-", "_"), TypeAttributes.Public | TypeAttributes.UnicodeClass);

            // ctor of a [HarmonyPatch()] attribute
            ConstructorInfo attrCtor = typeof(HarmonyPatch).GetConstructor( new Type[]{typeof(string), typeof(string), typeof(MethodType)} );
            CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {patchDef.className, patchDef.methodName, MethodType.Normal});
            tb.SetCustomAttribute(cab);

            if( patchDef.prefix != null ){
                MethodBuilder mb = tb.DefineMethod("Prefix",
                        MethodAttributes.Public | MethodAttributes.Static,
                        typeof(bool), new Type[] { patchDef.resultType.MakeByRefType() });
                if( patchDef.prefix.setResult != null ){
                    mb.DefineParameter(1, ParameterAttributes.None, "__result");
                    if( patchDef.resultType == typeof(float) ){
                        ILGenerator il = mb.GetILGenerator();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldc_R4, (float)patchDef.prefix.setResultObj);
                        il.Emit(OpCodes.Stind_R4);
                        il.Emit( patchDef.prefix.skipOriginal ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1 );
                        il.Emit(OpCodes.Ret);
                    } else {
                        Log.Error("[!] YADA: Unimplemented");
                    }
                }
            }

            if( patchDef.postfix != null ){
                MethodBuilder mb = tb.DefineMethod("Postfix",
                        MethodAttributes.Public | MethodAttributes.Static,
                        null, new Type[] { patchDef.resultType.MakeByRefType() });
                if( patchDef.postfix.setResult != null ){
                    mb.DefineParameter(1, ParameterAttributes.None, "__result");
                    if( patchDef.resultType == typeof(float) ){
                        ILGenerator il = mb.GetILGenerator();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldc_R4, (float)patchDef.postfix.setResultObj);
                        il.Emit(OpCodes.Stind_R4);
                        il.Emit(OpCodes.Ret);
                    } else {
                        Log.Error("[!] YADA: Unimplemented");
                    }
                }
            }

            tb.CreateType();
        }

        harmony.PatchAll(assembly);
    }
}
