using System;
using System.IO;
using System.Collections.Generic;
using Verse; // only for Log()

namespace zed_0xff.YADA;

public class Scanner {

    public const string rimignoreFname = ".rimignore";

    public static void ScanDir( string dir, Action<FileSystemInfo> action ){
        DirectoryInfo di = new DirectoryInfo(dir);
        ScanDir(di, action);
    }

    public static void ScanDir( DirectoryInfo dir, Action<FileSystemInfo> action ){
        var ignores = new List<string>();
        ScanDir(dir, action, ignores);
    }

    public static List<string> ReadIgnores(string fname){
        return ReadIgnores(new FileInfo(fname));
    }

    public static List<string> ReadIgnores(FileInfo fi){
        List<string> ignores = new List<string>();
        using (StreamReader sr = fi.OpenText()) {
            var s = "";
            while ((s = sr.ReadLine()) != null) {
                s = s.Trim();
                if( !s.StartsWith("#") && s != "" ){
                    ignores.Add(s);
                }
            }
        }
        return ignores;
    }

    public static void ScanDir(DirectoryInfo dir, Action<FileSystemInfo> action, List<string> ignores, bool logIgnored = false ){
        List<string> localIgnores = new List<string>();
        localIgnores.AddRange(ignores);
        foreach( FileInfo fi in dir.GetFiles(rimignoreFname) ){
            localIgnores.AddRange(ReadIgnores(fi));
        }
        Dictionary<string, FileSystemInfo> files = new Dictionary<string, FileSystemInfo>();
        foreach( var fi in dir.GetFileSystemInfos() ){
            files[fi.Name] = fi;
        }
        foreach( string glob in localIgnores ){
            if( glob.Contains("*") || glob.Contains("?") ){
                foreach( var fsi in dir.GetFileSystemInfos(glob) ){
                    if(files.ContainsKey(fsi.Name)){
                        if( logIgnored ) Log.Warning("[.] YADA: unnecessary " + (fsi is FileInfo ? "file" : "dir") + ": " + files[fsi.Name].FullName);
                        files.Remove(fsi.Name);
                    }
                }
            } else {
                if(files.ContainsKey(glob)){
                    if( logIgnored ) Log.Warning("[.] YADA: unnecessary " + (files[glob] is FileInfo ? "file" : "dir") + ": " + files[glob].FullName);
                    files.Remove(glob);
                }
            }
        }
        foreach( FileSystemInfo fsi in files.Values ){
            action(fsi);
            if ( fsi is DirectoryInfo di ){
                ScanDir(di, action, localIgnores);
            }
        }
    }
}
