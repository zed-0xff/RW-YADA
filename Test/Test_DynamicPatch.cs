using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using YADA;

// Dereference of a possibly null reference
#pragma warning disable CS8602

// cannot yet test actual patching because of https://github.com/pardeike/Harmony/issues/424 :(
class Test_DynamicPatch {

    static DynamicPatch new_dp(Type? resultType){
        PatchDef pd = new PatchDef();
        pd.className = "test";
        pd.methodName = "test";
        pd.resultType = resultType;

        Harmony harmony = new Harmony("YADA");
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

        var args = new object[]{ };
        Expect.Z(m.Invoke(null, args));
        Expect.Eq(args.Count(), 0);
    }

    static void test_null(bool? retValue = null){
        var dp = new_dp(typeof(string));
        var prefix = new PatchDef.Prefix();
        prefix.setResult = null;
        prefix.setResultObj = null;
        prefix.isResultNull = true;
        fixPrefix(prefix);
        var m0 = dp.MakeMethod("foo", prefix);
        Expect.NZ(m0);
        var type = dp.CreateType();

        var m = type.GetMethod("foo", BindingFlags.Static | BindingFlags.Public);
        Expect.NZ(m);

        var args = new object[]{ "hi" };
        Expect.Eq(m.Invoke(null, args), retValue);
        Expect.Eq(args[0], null);
    }

    static void test(Type type, object initial_value, object setResultObj, bool? retValue = null){
        var dp = new_dp(type);
        var prefix = new PatchDef.Prefix();
        prefix.setResult = setResultObj.ToString();
        prefix.setResultObj = setResultObj;
        fixPrefix(prefix);
        var m0 = dp.MakeMethod("foo", prefix);
        Expect.NZ(m0);

        var m = dp.CreateType().GetMethod("foo", BindingFlags.Static | BindingFlags.Public);
        Expect.NZ(m);

        var args = new object[]{ initial_value };
        Expect.Eq(m.Invoke(null, args), retValue);
        Expect.Eq(args[0], setResultObj);
        Expect.Eq(args[0].GetType(), type);
    }

    delegate void FixPrefixDelegate(PatchDef.Prefix prefix);
    static FixPrefixDelegate fixPrefix = delegate{};

    public static void Run(){
        Console.WriteLine("[.] Running Test_DynamicPatch");
        test_nop();

        test_null();
        test(typeof(string), "hi", "bye");
        test(typeof(float), 1.1f, 222f);
        test(typeof(int), 12, 555);
        test(typeof(long), 0L, 5555555555L);
        test(typeof(bool), false, true);
        test(typeof(bool), true, false);

        fixPrefix = delegate(PatchDef.Prefix prefix) { prefix.skipOriginal = true; };
        test_null(false);
        test(typeof(string), "hi", "bye", false);
        test(typeof(float), 1.1f, 222f, false);
        test(typeof(int), 12, 555, false);
        test(typeof(long), 0L, 5555555555L, false);
    }
}
