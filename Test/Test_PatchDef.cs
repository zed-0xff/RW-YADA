using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using zed_0xff.YADA;
using Verse;

class Test_PatchDef {

    private float test_float(){
        return 1.0f;
    }

    private int test_int(){
        return 1;
    }

    private string test_string(){
        return "hi";
    }

    private static void test_result_type(string typeName, Type t){
        PatchDef pd = new PatchDef();
        pd.className = "Test_PatchDef";
        pd.methodName = "test_" + typeName;
        pd.prefix = new PatchDef.Prefix();
        pd.PostLoad();

        Expect.Eq( pd.ConfigErrors().Count(), 0, delegate{
                foreach( var x in pd.ConfigErrors() ){
                    Console.WriteLine("    " + x);
                }
                } );

        Expect.Eq( pd.resultType, t );
    }

    private static void test_xml(){
        PatchDef pd = DirectXmlLoader.ItemFromXmlString<PatchDef>(@"
                <PatchDef>
                    <defName>foo</defName>
                    <className>Test_PatchDef</className>
                    <methodName>test_float</methodName>
                    <prefix>
                        <setResult>0.4</setResult>
                    </prefix>
                </PatchDef>
                ", "", false);

        Expect.Eq( pd.ConfigErrors().Count(), 0, delegate{ foreach( var x in pd.ConfigErrors() ){ Console.WriteLine("    " + x); } } );
        DefDatabase<PatchDef>.Add(pd);
    }

    public static void Run(){
        Console.WriteLine("[.] Running Test_PatchDef");
        test_result_type("int", typeof(int));
        test_result_type("float", typeof(float));
        test_result_type("string", typeof(string));
        test_xml();
    }
}
