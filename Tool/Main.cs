using System.IO;
using YADA;

string workshop_path = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
        "Library/Application Support/Steam/steamapps/workshop/content/294100"
        );

HashSet<string> goodExts = new HashSet<string>();
goodExts.Add(".afdesign"); // ?
goodExts.Add(".cs");
goodExts.Add(".csproj");
goodExts.Add(".dll");
goodExts.Add(".dds");  // MS DirectDraw surface
goodExts.Add(".html");
goodExts.Add(".jpg");
goodExts.Add(".md");
goodExts.Add(".mp3");
goodExts.Add(".ogg");
goodExts.Add(".p7s"); // .signature.p7s
goodExts.Add(".pdn"); // Paint.NET
goodExts.Add(".pl");
goodExts.Add(".png");
goodExts.Add(".psd");
goodExts.Add(".rsc"); // scenario
goodExts.Add(".sln");
goodExts.Add(".svg");
goodExts.Add(".txt");
goodExts.Add(".wav");
goodExts.Add(".xcf");
goodExts.Add(".xml");
goodExts.Add(".zip");
goodExts.Add(".LICENSE");

HashSet<string> goodNames = new HashSet<string>();
goodNames.Add(".rimignore");
goodNames.Add("Credit.txt");
goodNames.Add("License.txt");
goodNames.Add("LICENSE");
goodNames.Add("LICENSE.txt");
goodNames.Add("PublishedFileId.txt");
goodNames.Add("packages.config");
goodNames.Add("Rakefile");

DirectoryInfo root = new DirectoryInfo(workshop_path);
foreach( var di in root.GetDirectories() ){
    Scanner.ScanDir(di, delegate(FileSystemInfo fsi){
            if( fsi is FileInfo fi && !goodExts.Contains(fi.Extension.ToLower()) && !goodNames.Contains(fi.Name) ){
                Console.WriteLine(Path.GetRelativePath(workshop_path, fi.FullName));
            }
            }, Scanner.ReadIgnores(new FileInfo("../.rimignore")));
}
