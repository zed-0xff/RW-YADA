using System.Diagnostics;
using System.Text.Json; // not in net472/net48

// inventing a bicycle here

public static class Expect {
    public static void Eq(Type a, Type b){
        if( a != b ){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] " + a + " != " + b);
            Debug.Assert(false);
        }
    }

    public static void Eq(Object a, Object b, Action? dd = null){
        string ja = JsonSerializer.Serialize(a);
        string jb = JsonSerializer.Serialize(b);
        if( ja != jb ){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] " + ja + " != " + jb);
            if( dd != null ){
                dd();
            }
            Debug.Assert(false);
        }
    }

    public static void NZ(object? a, string msg = "object"){
        if( a == null ){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] " + msg + " is NULL");
            Debug.Assert(false);
        }
    }
}
