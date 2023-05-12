using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace zed_0xff.YADA;

[StaticConstructorOnStartup]
public class Init
{
    static Init()
    {
        Harmony harmony = new Harmony("zed_0xff.YADA");
        harmony.PatchAll();

        foreach( var x in DefDatabase<PatchDef>.AllDefsListForReading ){
            Log.Warning("[d] " + x);
        }

        // TODO: proper names
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ass"), AssemblyBuilderAccess.Run);
        var module = assembly.DefineDynamicModule("mod");
        var tb = module.DefineType("t0", TypeAttributes.Public | TypeAttributes.UnicodeClass);

        // ctor of a [HarmonyPatch()] attribute
        ConstructorInfo attrCtor = typeof(HarmonyPatch).GetConstructor( new Type[]{typeof(string), typeof(string), typeof(MethodType)} );
        CustomAttributeBuilder cab = new CustomAttributeBuilder( attrCtor, new object[] {"Need", "get_CurLevel", MethodType.Normal});
        tb.SetCustomAttribute(cab);

        MethodBuilder mb = tb.DefineMethod("Postfix",
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(float),
                    new Type[] { typeof(float) });

        mb.DefineParameter(1, ParameterAttributes.None, "__result");

        ILGenerator il = mb.GetILGenerator();
        il.Emit(OpCodes.Ldc_R4, 0.2f);
        il.Emit(OpCodes.Ret);

        tb.CreateType();

        harmony.PatchAll(assembly);
    }
}
