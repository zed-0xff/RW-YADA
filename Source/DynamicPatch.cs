using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace zed_0xff.YADA;

public class DynamicPatch {

    ///////////////////////////////////////////////////////////////////////////////////////////////

    private PatchDef patchDef;
    private TypeBuilder tb;
    private ModuleBuilder module;

    public DynamicPatch(PatchDef patchDef, ModuleBuilder module){
        this.patchDef = patchDef;
        this.module = module;

        tb = module.DefineType("YADA_Patch_" + patchDef.defName.Replace("-", "_"), TypeAttributes.Public | TypeAttributes.UnicodeClass);

        {
            // ctor of a [HarmonyPatch()] attribute
            ConstructorInfo attrCtor = typeof(HarmonyPatch).GetConstructor( new Type[]{typeof(string), typeof(string), typeof(MethodType)} );
            CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {patchDef.className, patchDef.methodName, MethodType.Normal});
            tb.SetCustomAttribute(cab);
        }
        if( !patchDef.HarmonyPriority.NullOrEmpty() ){
            int prio;
            if( patchDef.HarmonyPriority == "Priority.High" )
                prio = Priority.High;
            else if( patchDef.HarmonyPriority == "Priority.Low" )
                prio = Priority.Low;
            else if( patchDef.HarmonyPriority == "Priority.First" )
                prio = Priority.First;
            else if( patchDef.HarmonyPriority == "Priority.Last" )
                prio = Priority.Last;
            else if( patchDef.HarmonyPriority == "int.MinValue" )
                prio = int.MinValue;
            else if( patchDef.HarmonyPriority == "int.MaxValue" )
                prio = int.MaxValue;
            else if( !int.TryParse(patchDef.HarmonyPriority, out prio) ){
                prio = Priority.Normal;
            }
            ConstructorInfo attrCtor = typeof(HarmonyPriority).GetConstructor( new Type[]{typeof(int)} );
            CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {prio} );
            tb.SetCustomAttribute(cab);
        }
        if( !patchDef.HarmonyBefore.NullOrEmpty() ){
            ConstructorInfo attrCtor = typeof(HarmonyBefore).GetConstructor( new Type[]{typeof(string[])} );
            CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {patchDef.HarmonyBefore.ToArray()} );
            tb.SetCustomAttribute(cab);
        }
        if( !patchDef.HarmonyAfter.NullOrEmpty() ){
            ConstructorInfo attrCtor = typeof(HarmonyAfter).GetConstructor( new Type[]{typeof(string[])} );
            CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {patchDef.HarmonyAfter.ToArray()} );
            tb.SetCustomAttribute(cab);
        }
        if( patchDef.HarmonyDebug ){
            ConstructorInfo attrCtor = typeof(HarmonyDebug).GetConstructor( new Type[]{} );
            CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {} );
            tb.SetCustomAttribute(cab);
        }
    }

    public bool MakeMethods(){
        if( patchDef.prefix != null ){
            if( MakeMethod("Prefix", patchDef.prefix) == null )
                return false;
        }
        if( patchDef.postfix != null ){
            if( MakeMethod("Postfix", patchDef.postfix) == null )
                return false;
        }
        return true;
    }

    // TODO: make it better
    private bool emitResult(ILGenerator il, PatchDef.Anyfix anyfix){
        if( anyfix.setResultObj == null && !anyfix.isResultNull ) return true;

        il.Emit(OpCodes.Ldarg_0);

        if( anyfix.isResultNull ){
            // return null
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stind_Ref);
        } else if( patchDef.resultType == typeof(float) ){
            il.Emit(OpCodes.Ldc_R4, (float)anyfix.setResultObj);
            il.Emit(OpCodes.Stind_R4);
        } else if( patchDef.resultType == typeof(int) ){
            il.Emit(OpCodes.Ldc_I4, (int)anyfix.setResultObj);
            il.Emit(OpCodes.Stind_I4);
        } else if( patchDef.resultType == typeof(long) ){
            il.Emit(OpCodes.Ldc_I8, (long)anyfix.setResultObj);
            il.Emit(OpCodes.Stind_I8);
        } else if( patchDef.resultType == typeof(string) ){
            il.Emit(OpCodes.Ldstr, (string)anyfix.setResultObj);
            il.Emit(OpCodes.Stind_Ref);
        } else {
            Log.Error("[!] YADA: unsupported resultType " + patchDef.resultType);
            return false;
        }

        return true;
    }

    public MethodBuilder MakeMethod(string methodName, PatchDef.Anyfix anyfix){
//        var mstub = AccessTools.Method(this.GetType(), "stub", new Type[]{ patchDef.resultType.MakeByRefType() } );
//        if( mstub == null ){
//            Log.Error("[!] YADA: cannot get stub method for " + patchDef.resultType);
//            return null;
//        }
//
//        var instructions = PatchProcessor.GetOriginalInstructions(mstub);
        bool skipOriginal = anyfix is PatchDef.Prefix prefix && prefix.skipOriginal;
        bool hasArgument = anyfix.setResultObj != null || anyfix.isResultNull;

        Type[] args = hasArgument ? (new Type[] { patchDef.resultType.MakeByRefType() }) : new Type[]{};
        Type retType = skipOriginal ? typeof(bool) : null;
        MethodBuilder mb = tb.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, retType, args);

        if( hasArgument ){
            mb.DefineParameter(1, ParameterAttributes.None, "__result");
        }

        ILGenerator il = mb.GetILGenerator();
        if(!emitResult(il, anyfix)) return null;
        if( skipOriginal ){
            il.Emit( OpCodes.Ldc_I4_0 ); // returns false in Prefix()
        }
        il.Emit(OpCodes.Ret);

        return mb;
    }

    public Type CreateType(){
        return tb.CreateType();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    public static void PatchAll(){
        Harmony harmony = new Harmony("zed_0xff.YADA");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("yada_dyn_ass"), AssemblyBuilderAccess.Run);
        ModuleBuilder module = assembly.DefineDynamicModule("yada_dyn_mod");

        foreach( PatchDef patchDef in DefDatabase<PatchDef>.AllDefsListForReading ){
            if( patchDef.ConfigErrors().Any() ) continue;

            var dp = new DynamicPatch(patchDef, module);
            if( dp.MakeMethods() ){
                dp.CreateType();
            }
        }

        harmony.PatchAll(assembly);
    }
}
