using HarmonyLib;
using System.IO;
using System;
using Verse;

namespace zed_0xff.YADA;

[HarmonyPatch(typeof(ModMetaData), nameof(ModMetaData.PrepareForWorkshopUpload))]
static class Patch_PrepareForWorkshopUpload {

    // there's no Path.GetRelativePath in net472 :(
    static string GetRelativePath(string parent, string child){
        if( child.StartsWith(parent) ){
            return child.Substring(parent.Length+1);
        }
        throw new Exception("Cannot get relative path for " + child + " in " + parent);
    }

    static void Postfix(ModMetaData __instance){
        if( Patch__Dialog_ConfirmModUpload__DoWindowContents.lastScan.rootDir != __instance.RootDir.FullName )
            return;

        // from Scenario.cs
        string path = __instance.Name + Rand.RangeInclusive(10000, 99999);
        string tempUploadDir = Path.Combine(GenFilePaths.TempFolderPath, path);
        DirectoryInfo directoryInfo = new DirectoryInfo(tempUploadDir);
        if (directoryInfo.Exists)
        {
            directoryInfo.Delete(recursive: true);
        }
        directoryInfo.Create(); // will be deleted by rimworld's Root.Shutdown()

        // use vanilla upload
        Patch__Dialog_ConfirmModUpload__DoWindowContents.lastScan.tempUploadDir = null;

        try {
            bool allOK = true;
            Scanner.ScanDir(__instance.RootDir, delegate(FileSystemInfo fsi){
                    string target = Path.Combine(tempUploadDir, GetRelativePath(__instance.RootDir.FullName, fsi.FullName));
                    if( fsi is DirectoryInfo di ){
                        Log.Warning("[d] mkdir " + target);
                        Directory.CreateDirectory(target);
                    } else if( fsi is FileInfo fi ){
                        Log.Warning("[d] cp " + fi.FullName + " " + target);
                        File.Copy(fi.FullName, target);
                    } else {
                        Log.Warning("[?] YADA: unexpected " + fsi);
                        allOK = false;
                    }
                    });
            if( allOK ){
                // use patched upload
                Patch__Dialog_ConfirmModUpload__DoWindowContents.lastScan.tempUploadDir = tempUploadDir;
            }
        } catch (Exception ex){
            Log.Error("[!] YADA: " + ex);
        }
    }
}

[HarmonyPatch(typeof(ModMetaData), nameof(ModMetaData.GetWorkshopUploadDirectory))]
static class Patch_GetWorkshopUploadDirectory {
    static void Postfix(ref ModMetaData __instance, ref DirectoryInfo __result){
        if( Patch__Dialog_ConfirmModUpload__DoWindowContents.lastScan.rootDir != __instance.RootDir.FullName )
            return;
        if( Patch__Dialog_ConfirmModUpload__DoWindowContents.lastScan.tempUploadDir == null )
            return;
        if( Patch__Dialog_ConfirmModUpload__DoWindowContents.lastScan.tempUploadDir == "" )
            return;

        __result = new DirectoryInfo(Patch__Dialog_ConfirmModUpload__DoWindowContents.lastScan.tempUploadDir);
    }
}
