using System;
using System.IO;
using System.Collections.Generic;
//using Verse; // only for Log()

namespace zed_0xff.YADA;

public class Scanner {

    public const string rimignoreFname = ".rimignore";

    public static void ScanDir( string dir, Action<FileSystemInfo> action ){
        DirectoryInfo di = new DirectoryInfo(dir);
        ScanDir(di, action);
    }

    public static void ScanDir( DirectoryInfo dir, Action<FileSystemInfo> action ){
        var ignores = new List<string>();
        _ScanDir(dir, action, ignores);
    }

    public static void _ScanDir(DirectoryInfo dir, Action<FileSystemInfo> action, List<string> ignores ){
        List<string> localIgnores = new List<string>();
        localIgnores.AddRange(ignores);
        foreach( FileInfo fi in dir.GetFiles(rimignoreFname) ){
            using (StreamReader sr = fi.OpenText()) {
                var s = "";
                while ((s = sr.ReadLine()) != null) {
                    s = s.Trim();
                    if( !s.StartsWith("#") && s != "" ){
                        localIgnores.Add(s);
                    }
                }
            }
        }
        Dictionary<string, FileSystemInfo> files = new Dictionary<string, FileSystemInfo>();
        foreach( var fi in dir.GetFileSystemInfos() ){
            files[fi.Name] = fi;
        }
        foreach( string glob in localIgnores ){
            if( glob.Contains("*") || glob.Contains("?") ){
                foreach( var fsi in dir.GetFileSystemInfos(glob) ){
                    files.Remove(fsi.Name);
                }
            } else {
                files.Remove(glob);
            }
        }
        foreach( FileSystemInfo fsi in files.Values ){
            action(fsi);
            if ( fsi is DirectoryInfo di ){
                _ScanDir(di, action, localIgnores);
            }
        }
    }
}
