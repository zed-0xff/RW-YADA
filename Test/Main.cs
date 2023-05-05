using System.IO;
using zed_0xff.YADA;

// there's no Path.GetRelativePath in net472 :(
string GetRelativePath(string parent, string child){
    if( child.StartsWith(parent) ){
        return child.Substring(parent.Length+1);
    }
    throw new Exception("Cannot get relative path for " + child + " in " + parent);
}

{
    List<String> fnames = new List<String>();
    DirectoryInfo di = new DirectoryInfo(".");
    Scanner.ScanDir(di, delegate(FileSystemInfo fi){
            fnames.Add(GetRelativePath(di.FullName, fi.FullName));
            });
    fnames.Sort();
    Expect.Eq(fnames, new List<String>{ ".rimignore","Expect.cs","Main.cs","Test.csproj","testdir1","testdir1/.rimignore","testdir1/foo" });
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("[=] ALL OK!");

namespace Verse {
    public static class Log {
        public static void Warning(string s){
            //Console.WriteLine(s);
        }
    }
}

