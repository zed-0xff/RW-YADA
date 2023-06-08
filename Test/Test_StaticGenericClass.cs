using System.Collections;
using System.Reflection;

public class C0 {
    private static HashSet<IDictionary> allDicts = new HashSet<IDictionary>();

    protected static void registerDict(IDictionary d){
        allDicts.Add(d);
    }

    public static void ClearAll(){
        foreach( var x in allDicts ){
            x.Clear();
        }
    }
}

public class Cache<T> : C0 {
    static Dictionary <int, T> dict = new Dictionary<int, T>();

    public static void Add(int k, T v){
        registerDict(dict);
        dict[k] = v;
    }

    public static T Get(int k){
        return dict[k];
    }

    public static int Size(){
        return dict.Count();
    }

    public static void Clear(){
        dict.Clear();
    }
}

class Test_StaticGenericClass {
    public static void Run(){
        Console.WriteLine("[.] Running Test_StaticGenericClass");
        Cache<int>.Add(123, 456);
        Cache<float>.Add(123, 45);
        Cache<string>.Add(123, "foo");

        Expect.Eq(Cache<int>.Size(), 1);
        Expect.Eq(Cache<float>.Size(), 1);
        Expect.Eq(Cache<string>.Size(), 1);

        Expect.Eq(Cache<int>.Get(123), 456);
        Expect.Eq(Cache<float>.Get(123), 45);
        Expect.Eq(Cache<string>.Get(123), "foo");

        C0.ClearAll();

        Expect.Eq(Cache<int>.Size(), 0);
        Expect.Eq(Cache<float>.Size(), 0);
        Expect.Eq(Cache<string>.Size(), 0);
    }
}
