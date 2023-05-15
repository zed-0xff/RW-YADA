using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace zed_0xff.YADA;

public class DynamicPatch {

    private PatchDef patchDef;
    private TypeBuilder tb;
    private ModuleBuilder module;

    public static List<FieldInfo> dynamicSettings = new List<FieldInfo>();
    private static int uniqNameCounter = 0;

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
// too new?
//        if( !patchDef.HarmonyPatchCategory.NullOrEmpty() ){
//            ConstructorInfo attrCtor = typeof(HarmonyPatchCategory).GetConstructor( new Type[]{typeof(string)} );
//            CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {patchDef.HarmonyPatchCategory} );
//            tb.SetCustomAttribute(cab);
//        }
        if( patchDef.HarmonyDebug ){
            ConstructorInfo attrCtor = typeof(HarmonyDebug).GetConstructor( new Type[]{} );
            CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {} );
            tb.SetCustomAttribute(cab);
        }
    }

    private FieldInfo dsCheckbox = null;

    public bool MakeMethods(){
        if( patchDef.debugSettingsCheckbox != null ){
            string ds_className = "YADA_DynamicSettings_" + uniqNameCounter++;
            string ds_fieldName = patchDef.defName;
            TypeBuilder dynamicSettingsBuilder = module.DefineType(ds_className, TypeAttributes.Public | TypeAttributes.UnicodeClass);
            FieldBuilder fb = dynamicSettingsBuilder.DefineField(ds_fieldName, typeof(bool), FieldAttributes.Public | FieldAttributes.Static);
            if( !patchDef.label.NullOrEmpty() ){
                ConstructorInfo attrCtor = typeof(FieldLabel).GetConstructor( new Type[]{typeof(string)} );
                CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {patchDef.label} );
                fb.SetCustomAttribute(cab);
            }
            {
                ConstructorInfo attrCtor = typeof(FieldCategory).GetConstructor( new Type[]{typeof(string)} );
                CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {patchDef.debugSettingsCheckbox.category } );
                fb.SetCustomAttribute(cab);
            }
            Type dynamicSettingsContainer = dynamicSettingsBuilder.CreateType();
            dsCheckbox = dynamicSettingsContainer.GetField(ds_fieldName);
            dsCheckbox.SetValue( null, patchDef.debugSettingsCheckbox.defaultValue );
            dynamicSettings.Add(dsCheckbox);
        }
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
        } else if( patchDef.resultType == typeof(bool) ){
            il.Emit( (bool)anyfix.setResultObj ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0 );
            il.Emit(OpCodes.Stind_I1);
        } else {
            Log.Error("[!] YADA: unsupported resultType " + patchDef.resultType);
            return false;
        }

        return true;
    }

    public MethodBuilder MakeMethodFromOpcodes(string methodName, PatchDef.Anyfix anyfix){
        var parser = new Parser(anyfix, patchDef);
        if( !parser.ParseArguments() ){
            Log.Error("[!] YADA: " + parser.error);
            return null;
        }

        bool skipOriginal = anyfix is PatchDef.Prefix prefix && prefix.skipOriginal;
        Type retType = skipOriginal ? typeof(bool) : null;
        MethodBuilder mb = tb.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, retType, parser.argTypes.ToArray());

        for( int i=0; i<parser.argNames.Count(); i++ ){
            mb.DefineParameter(i+1, ParameterAttributes.None, parser.argNames[i]);
            if( patchDef.HarmonyDebug && Prefs.DevMode ){
                Log.Message("[d] YADA: arg #" + i + ": " + parser.argTypes[i] + " " + parser.argNames[i]);
            }
        }

        ILGenerator il = mb.GetILGenerator();
        emitCommonHead(il);
        if( !parser.ParseOpcodes(il) ){
            Log.Error("[!] YADA: " + parser.error);
            return null;
        }
        emitCommonTail(il, skipOriginal);

        return mb;
    }

    Label lret;

    void emitCommonHead(ILGenerator il){
        lret = il.DefineLabel();

        if( dsCheckbox != null ){
            il.Emit( OpCodes.Ldsfld, dsCheckbox );
            il.Emit( OpCodes.Brfalse, lret );
        }
    }

    void emitCommonTail(ILGenerator il, bool skipOriginal){
        if( skipOriginal ){
            il.Emit( OpCodes.Ldc_I4_0 ); // returns false in Prefix()
        }
        il.Emit(OpCodes.Ret);

        // we'll get here if checkbox is OFF
        il.MarkLabel(lret);
        if( skipOriginal ){
            il.Emit( OpCodes.Ldc_I4_1 ); // returns true in Prefix()
        }
        il.Emit(OpCodes.Ret);
    }

    public MethodBuilder MakeMethod(string methodName, PatchDef.Anyfix anyfix){
        if( anyfix.opcodes != null )
            return MakeMethodFromOpcodes(methodName, anyfix);

        bool skipOriginal = anyfix is PatchDef.Prefix prefix && prefix.skipOriginal;
        bool hasArgument = anyfix.setResultObj != null || anyfix.isResultNull;

        Type[] args = hasArgument ? (new Type[] { patchDef.resultType.MakeByRefType() }) : new Type[]{};
        Type retType = skipOriginal ? typeof(bool) : null;
        MethodBuilder mb = tb.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, retType, args);

        if( hasArgument ){
            mb.DefineParameter(1, ParameterAttributes.None, "__result");
        }

        ILGenerator il = mb.GetILGenerator();
        emitCommonHead(il);
        if(!emitResult(il, anyfix)) return null;
        emitCommonTail(il, skipOriginal);

        return mb;
    }

    public Type CreateType(){
        return tb.CreateType();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////

    private static void debugLogAllMethodsFrom(Type t){
        StringBuilder sb = new StringBuilder();
        foreach( var c in t.CustomAttributes ){
            sb.AppendLineIfNotEmpty().Append("[d] " + c);
        }
        sb.AppendLineIfNotEmpty().Append("[d] " + t);
        sb.AppendLineIfNotEmpty();

        foreach( MethodInfo mi in t.GetMethods(AccessTools.allDeclared)){
            Utils.Disasm(mi, sb);
        }
        Log.Message(sb.ToString());
    }

    public static void PatchAll(){
        Harmony harmony = null;
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("yada_dyn_ass"), AssemblyBuilderAccess.Run);
        ModuleBuilder module = assembly.DefineDynamicModule("yada_dyn_mod");

        foreach( PatchDef patchDef in DefDatabase<PatchDef>.AllDefsListForReading ){
            if( !patchDef.enabled ) continue;
            if( patchDef.ConfigErrors().Any() ) continue;

            if( Prefs.DevMode ){
                Log.Message("[d] YADA: applying dynamic patch " + patchDef.defName + " from " + patchDef.modContentPack);
            }

            var dp = new DynamicPatch(patchDef, module);
            if( dp.MakeMethods() ){
                var t = dp.CreateType();
                if( patchDef.HarmonyDebug && Prefs.DevMode ){
                    debugLogAllMethodsFrom(t);
                }
                string newHarmonyId = patchDef.modContentPack.PackageIdPlayerFacing;
                if( harmony == null || harmony.Id != newHarmonyId ){
                    harmony = new Harmony(newHarmonyId);
                }
                harmony.CreateClassProcessor(t).Patch();
            }
        }
    }
}
