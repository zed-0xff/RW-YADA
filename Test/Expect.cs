using System.Diagnostics;
using System.Text.Json; // not in net472/net48

// inventing a bicycle here

public static class Expect {
    public static void Eq(Type a, Type b){
        if( a != b ){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] " + a + " != " + b);
            System.Environment.Exit(1);
        }
    }

    public static void Eq(object? a, object? b, Action? dd = null){
        string ja = JsonSerializer.Serialize(a);
        string jb = JsonSerializer.Serialize(b);
        if( ja != jb ){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] " + ja + " != " + jb);
            if( dd != null ){
                dd();
            }
            System.Environment.Exit(1);
        }
    }

    public static void Z(object? a, string msg = "object"){
        if( a != null ){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] " + msg + " is NOT NULL");
            System.Environment.Exit(1);
        }
    }

    public static void NZ(object? a, string msg = "object"){
        if( a == null ){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] " + msg + " is NULL");
            System.Environment.Exit(1);
        }
    }
}
