using HarmonyLib;
using RimWorld;
using System.IO;
using UnityEngine;
using Verse;

namespace zed_0xff.YADA;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(Dialog_ConfirmModUpload), nameof(Dialog_ConfirmModUpload.DoWindowContents))]
static class Patch__Dialog_ConfirmModUpload__DoWindowContents {

    private static readonly Texture2D WarningIcon = Resources.Load<Texture2D>("Textures/UI/Widgets/Warning");

    public struct ScanResult {
        public bool hasRimIgnore;
        public int totalFiles;
        public long totalSize;
        public int badFiles;
        public long badSize;

        // for use in Patch_ModMetaData
        public string rootDir;
        public string tempUploadDir;

        public int step;

        public ScanResult(string r){
            rootDir = r;
        }
    }

    static string formatSize(long totalSize){
        if( totalSize > 1024*1024 ){
            return (totalSize / 1024f / 1024f).ToString("F1") + " MB";
        } else if( totalSize > 1024 ){
            return (totalSize / 1024) + " KB";
        } else {
            return totalSize + " bytes";
        }
    }

    // need to pass it from Prefix to Postfix
    public static ScanResult lastScan;
    public static int lastWindowID;

    static void scanMod(DirectoryInfo rootDir){
        if( lastWindowID == Find.WindowStack.currentlyDrawnWindow.ID )
            return;
        lastWindowID = Find.WindowStack.currentlyDrawnWindow.ID;

        lastScan = new ScanResult(rootDir.FullName);
        Scanner.ScanDir(rootDir, delegate(FileSystemInfo fsi){
                if( fsi is FileInfo fi) {
                  lastScan.totalFiles++;
                  lastScan.totalSize += fi.Length;
                  lastScan.hasRimIgnore = lastScan.hasRimIgnore || (fsi is FileInfo && fsi.Name == Scanner.rimignoreFname);
                }
                });
        lastScan.hasRimIgnore = lastScan.hasRimIgnore || File.Exists(Path.Combine(rootDir.ToString(), Scanner.rimignoreFname));
        lastScan.step++;
    }

    static void Prefix(Rect inRect, ModMetaData ___mod, Dialog_ConfirmModUpload __instance){
        scanMod(___mod.RootDir);
        if( !lastScan.hasRimIgnore ){
            __instance.buttonCText = ("Add " + Scanner.rimignoreFname).Colorize(new Color(0,0.8f,0));
            __instance.buttonCAction = delegate{
                File.Copy(
                        Path.Combine(ModConfig.RootDir, Scanner.rimignoreFname),
                        Path.Combine(___mod.RootDir.FullName, Scanner.rimignoreFname)
                        );
                Find.WindowStack.Add( new Dialog_ConfirmModUpload(___mod, __instance.buttonAAction) );
            };
        }
    }

    static void Postfix(Rect inRect, ref ModMetaData ___mod){
        Vector2 topLeft = new Vector2(inRect.x + 10f, inRect.height - 35 - 24 - 10);
        GenUI.SetLabelAlign(TextAnchor.MiddleRight);

        var rect = new Rect(topLeft.x + inRect.width / 2, topLeft.y + (24 - Text.LineHeight) / 2f, inRect.width / 2f - 24, 24);

        string tipText = "Total " + lastScan.totalSize + " bytes in " + lastScan.totalFiles + " files";
        if( lastScan.hasRimIgnore ){
            GUI.color = Color.green;
        } else {
            if( lastScan.step == 1 ){
                var sr1 = new ScanResult(null);
                Scanner.ScanDir(___mod.RootDir, delegate(FileSystemInfo fsi){
                        if( fsi is FileInfo fi) {
                            sr1.totalFiles++;
                            sr1.totalSize += fi.Length;
                        }
                        }, Scanner.ReadIgnores(Path.Combine(ModConfig.RootDir, Scanner.rimignoreFname)), logIgnored: true);

                lastScan.badSize = lastScan.totalSize - sr1.totalSize;
                lastScan.badFiles = lastScan.totalFiles - sr1.totalFiles;
                lastScan.step++;
            }
            if( lastScan.badSize > 0 || lastScan.badFiles > 0 ){
                var iconRect = new Rect(topLeft.x + inRect.width - WarningIcon.width - 24, topLeft.y + (24 - Text.LineHeight) / 2f, WarningIcon.width, 24);
                Widgets.DrawTextureFitted(iconRect, WarningIcon, 0.9f);
                rect.width -= WarningIcon.width;
                GUI.color = Color.red;

                tipText += "\n\n" + (lastScan.badSize + " bytes in " + lastScan.badFiles + " unnecessary files").Colorize(Color.red);
                tipText += "\n(filenames are listed in debug log)";
                tipText += "\n\nAdd .rimignore to skip uploading these files";
            }
        }

        Widgets.Label(rect, "Size".Translate().CapitalizeFirst() + ": " + formatSize(lastScan.totalSize));
        GUI.color = Color.white;

        TooltipHandler.TipRegion(rect, tipText);
        GenUI.ResetLabelAlign();
    }
}
