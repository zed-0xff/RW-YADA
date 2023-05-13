using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using zed_0xff.YADA;

// Dereference of a possibly null reference
#pragma warning disable CS8602

// cannot yet test actual patching because of https://github.com/pardeike/Harmony/issues/424 :(
class Test_DynamicPatch {

    static DynamicPatch new_dp(Type? resultType){
        PatchDef pd = new PatchDef();
        pd.className = "test";
        pd.methodName = "test";
        pd.resultType = resultType;

        Harmony harmony = new Harmony("zed_0xff.YADA");
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("yada_dyn_ass"), AssemblyBuilderAccess.Run);
        ModuleBuilder mb = assembly.DefineDynamicModule("yada_dyn_mod");

        return new DynamicPatch(pd, mb);
    }

    static void test_nop(){
        var dp = new_dp(null);
        var prefix = new PatchDef.Prefix();
        var m0 = dp.MakeMethod("foo", prefix);
        Expect.NZ(m0);
        var type = dp.CreateType();

        var m = type.GetMethod("foo", BindingFlags.Static | BindingFlags.Public);
        Expect.NZ(m);

        m.Invoke(null, new object[]{ });
    }

    static void test_int(){
        var dp = new_dp(typeof(int));
        var prefix = new PatchDef.Prefix();
        prefix.setResult = "1";
        prefix.setResultObj = 1;
        var m0 = dp.MakeMethod("foo", prefix);
        Expect.NZ(m0);
        var type = dp.CreateType();

        var m = type.GetMethod("foo", BindingFlags.Static | BindingFlags.Public);
        Expect.NZ(m);

        var args = new object[]{ 33 };
        m.Invoke(null, args);
        Expect.Eq(args[0], 1);
    }

    static void test_float(){
        var dp = new_dp(typeof(float));
        var prefix = new PatchDef.Prefix();
        prefix.setResult = "1.2";
        prefix.setResultObj = 1.2f;
        var m0 = dp.MakeMethod("foo", prefix);
        Expect.NZ(m0);
        var type = dp.CreateType();

        var m = type.GetMethod("foo", BindingFlags.Static | BindingFlags.Public);
        Expect.NZ(m);

        var args = new object[]{ 5.5f };
        m.Invoke(null, args);
        Expect.Eq(args[0], 1.2f);
    }

    static void test_string(){
        var dp = new_dp(typeof(string));
        var prefix = new PatchDef.Prefix();
        prefix.setResult = "hi";
        prefix.setResultObj = "hi";
        var m0 = dp.MakeMethod("foo", prefix);
        Expect.NZ(m0);
        var type = dp.CreateType();

        var m = type.GetMethod("foo", BindingFlags.Static | BindingFlags.Public);
        Expect.NZ(m);

        var args = new object[]{ "bye" };
        m.Invoke(null, args);
        Expect.Eq(args[0], "hi");
    }

    public static void Run(){
        Console.WriteLine("[.] Running Test_DynamicPatch");
        test_nop();
        test_int();
        test_float();
        test_string();
    }
}
