using System.IO;
using YADA;

class Test_RimIgnore {
    // there's no Path.GetRelativePath in net472 :(
    static string GetRelativePath(string parent, string child){
        if( child.StartsWith(parent) ){
            return child.Substring(parent.Length+1);
        }
        throw new Exception("Cannot get relative path for " + child + " in " + parent);
    }

    public void test(){
    }

    public static void Run(){
        Console.WriteLine("[.] Running Test_RimIgnore");
        List<String> fnames = new List<String>();
        DirectoryInfo di = new DirectoryInfo(".");
        Scanner.ScanDir(di, delegate(FileSystemInfo fi){
                fnames.Add(GetRelativePath(di.FullName, fi.FullName));
                });
        fnames.Sort();
        Expect.Eq(fnames, new List<String>{ ".rimignore","Expect.cs","Main.cs","Test.csproj","testdir1","testdir1/.rimignore","testdir1/foo" });
    }
}
