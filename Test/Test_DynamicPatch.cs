using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using zed_0xff.YADA;

class Test_DynamicPatch {

    // cannot yet test actual patching because of https://github.com/pardeike/Harmony/issues/424 :(
    public static void Run(){
        Console.WriteLine("[.] Running Test_DynamicPatch");

        PatchDef pd = new PatchDef();
        pd.className = "test";
        pd.methodName = "test";
//        pd.resultType = typeof(int);

        Harmony harmony = new Harmony("zed_0xff.YADA");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("yada_dyn_ass"), AssemblyBuilderAccess.Run);
        ModuleBuilder mb = assembly.DefineDynamicModule("yada_dyn_mod");

        var dp = new DynamicPatch(pd, mb);
        var prefix = new PatchDef.Prefix();
//        prefix.setResult = "1";
//        prefix.setResultObj = 1;
        var m = dp.MakeMethod("foo", prefix);
        Expect.NZ(m);
        dp.CreateType();

//        int x = 33;
        m.Invoke(null, new object[]{ });
    }
}
