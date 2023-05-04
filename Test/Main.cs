using System.IO;
using zed_0xff.YADA;

{
    List<String> fnames = new List<String>();
    DirectoryInfo di = new DirectoryInfo(".");
    Scanner.ScanDir(di, delegate(FileSystemInfo fi){
            fnames.Add(Path.GetRelativePath(di.FullName, fi.FullName));
            });
    fnames.Sort();
    Expect.Eq(fnames, new List<String>{ ".rimignore","Expect.cs","Main.cs","Test.csproj","testdir1","testdir1/.rimignore","testdir1/foo" });
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("[=] ALL OK!");
