using System.Diagnostics;
using System.Text.Json;

// inventing a bike here

public static class Expect {
    public static void Eq(Object a, Object b){
        string ja = JsonSerializer.Serialize(a);
        string jb = JsonSerializer.Serialize(b);
        if( ja != jb ){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[!] " + ja + " != " + jb);
            Debug.Assert(false);
        }
    }
}
