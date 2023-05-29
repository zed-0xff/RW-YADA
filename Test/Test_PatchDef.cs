using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using YADA;
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

    private bool test_bool(){
        return false;
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
        {
            PatchDef pd = DirectXmlLoader.ItemFromXmlString<PatchDef>(@"
                    <PatchDef>
                        <className>Test_PatchDef</className>
                        <methodName>test_float</methodName>
                        <prefix>
                            <setResult>0.4</setResult>
                        </prefix>
                    </PatchDef>
                    ", "", false);
            pd.PostLoad();

            Expect.Eq( pd.ConfigErrors().Count(), 0, delegate{ foreach( var x in pd.ConfigErrors() ){ Console.WriteLine("    " + x); } } );
            Expect.Eq( pd.postfix, null );
            Expect.Eq( pd.prefix.isResultNull, false );
            Expect.Eq( pd.prefix.setResultObj, 0.4f );

            DefDatabase<PatchDef>.Add(pd);
        }
        {
            PatchDef pd = DirectXmlLoader.ItemFromXmlString<PatchDef>(@"
                    <PatchDef>
                        <className>Test_PatchDef</className>
                        <methodName>test_string</methodName>
                        <prefix>
                            <skipOriginal>true</skipOriginal>
                        </prefix>
                    </PatchDef>
                    ", "", false);
            pd.PostLoad();
            Expect.Eq( pd.prefix.skipOriginal, true );
            Expect.Eq( pd.prefix.setResult, null );
            Expect.Eq( pd.prefix.setResultObj, null );
            Expect.Eq( pd.prefix.isResultNull, false );
        }
        {
            PatchDef pd = DirectXmlLoader.ItemFromXmlString<PatchDef>(@"
                    <PatchDef>
                        <className>Test_PatchDef</className>
                        <methodName>test_string</methodName>
                        <prefix>
                            <setResult IsNull=""true"" />
                        </prefix>
                    </PatchDef>
                    ", "", false);
            pd.PostLoad();
            Expect.Eq( pd.prefix.setResult, null );
            Expect.Eq( pd.prefix.setResultObj, null );
            Expect.Eq( pd.prefix.isResultNull, true ); // <-------------
        }
    }

    public static void Run(){
        Console.WriteLine("[.] Running Test_PatchDef");
        test_result_type("int", typeof(int));
        test_result_type("float", typeof(float));
        test_result_type("string", typeof(string));
        test_result_type("bool", typeof(bool));
        test_xml();
    }
}
