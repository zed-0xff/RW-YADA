using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

class Test_Harmony {
    public static void Run(){
        Console.WriteLine("[.] Running Test_Harmony");
        Harmony harmony = new Harmony("YADA");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("yada_dyn_ass"), AssemblyBuilderAccess.Run);
        ModuleBuilder module = assembly.DefineDynamicModule("yada_dyn_mod");

        harmony.PatchAll(assembly); // nop, should just succeed
    }
}
