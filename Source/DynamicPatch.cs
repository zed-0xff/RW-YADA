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

        // ctor of a [HarmonyPatch()] attribute
        ConstructorInfo attrCtor = typeof(HarmonyPatch).GetConstructor( new Type[]{typeof(string), typeof(string), typeof(MethodType)} );
        CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {patchDef.className, patchDef.methodName, MethodType.Normal});
        tb.SetCustomAttribute(cab);
        // TODO: more custom attrs
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
    private bool emitResult(ILGenerator il, PatchDef.Somefix somefix){
        // FIXME: how to return null?
        if( somefix.setResult == null ) return true;

        if( patchDef.resultType == typeof(float) ){
            il.Emit(OpCodes.Ldc_R4, (float)somefix.setResultObj);
            il.Emit(OpCodes.Stind_R4);
        } else if( patchDef.resultType == typeof(int) ){
            il.Emit(OpCodes.Ldc_I4, (int)somefix.setResultObj);
            il.Emit(OpCodes.Stind_I4);
        } else if( patchDef.resultType == typeof(long) ){
            il.Emit(OpCodes.Ldc_I8, (long)somefix.setResultObj);
            il.Emit(OpCodes.Stind_I8);
        } else if( patchDef.resultType == typeof(string) ){
            il.Emit(OpCodes.Ldstr, (string)somefix.setResultObj);
            il.Emit(OpCodes.Stind_Ref);
        } else {
            Log.Error("[!] YADA: unsupported resultType " + patchDef.resultType);
            return false;
        }

        return true;
    }

    public MethodBuilder MakeMethod(string methodName, PatchDef.Somefix somefix){
//        var mstub = AccessTools.Method(this.GetType(), "stub", new Type[]{ patchDef.resultType.MakeByRefType() } );
//        if( mstub == null ){
//            Log.Error("[!] YADA: cannot get stub method for " + patchDef.resultType);
//            return null;
//        }
//
//        var instructions = PatchProcessor.GetOriginalInstructions(mstub);
        bool skipOriginal = somefix is PatchDef.Prefix prefix && prefix.skipOriginal;

        Type[] args = (somefix.setResult != null) ? (new Type[] { patchDef.resultType.MakeByRefType() }) : new Type[]{};
        Type retType = skipOriginal ? typeof(bool) : null;
        MethodBuilder mb = tb.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, retType, args);

        if( somefix.setResult != null ){
            mb.DefineParameter(1, ParameterAttributes.None, "__result");
        }

        ILGenerator il = mb.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        if(!emitResult(il, somefix)) return null;
        if( skipOriginal ){
            il.Emit( OpCodes.Ldc_I4_0 ); // returns false in Prefix()
        }
        il.Emit(OpCodes.Ret);

        return mb;
    }

    public void CreateType(){
        tb.CreateType();
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
