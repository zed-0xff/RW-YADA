using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEngine;
using Verse;

namespace zed_0xff.YADA;

[HarmonyPatch(typeof(Dialog_ConfirmModUpload), nameof(Dialog_ConfirmModUpload.DoWindowContents))]
static class Patch__Dialog_ConfirmModUpload__DoWindowContents {

    public struct ScanResult {
        public bool hasRimIgnore;
        public int totalFiles;
        public long totalSize;
        public int badFiles;
        public long badSize;
        public string rootDir;
        public string tempUploadDir;

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

    static void Prefix(Rect inRect, ref ModMetaData ___mod, ref Dialog_ConfirmModUpload __instance){
        lastScan = new ScanResult(___mod.RootDir.FullName);
        Scanner.ScanDir(___mod.RootDir, delegate(FileSystemInfo fsi){
                if( fsi is FileInfo fi) {
                  lastScan.totalFiles++;
                  lastScan.totalSize += fi.Length;
                  lastScan.hasRimIgnore = lastScan.hasRimIgnore || (fsi is FileInfo && fsi.Name == Scanner.rimignoreFname);
                }
                });
        lastScan.hasRimIgnore = lastScan.hasRimIgnore || File.Exists(Path.Combine(___mod.RootDir.ToString(), Scanner.rimignoreFname));
        if( !lastScan.hasRimIgnore ){
            __instance.buttonCText = ("Add " + Scanner.rimignoreFname).Colorize(new Color(0,0.8f,0));
        }
    }

    static void Postfix(Rect inRect, ref ModMetaData ___mod){
        Vector2 topLeft = new Vector2(inRect.x + 10f, inRect.height - 35 - 24 - 10);
        GenUI.SetLabelAlign(TextAnchor.MiddleRight);

        string sizeString = formatSize(lastScan.totalSize);
        var r = new Rect(topLeft.x + inRect.width / 2, topLeft.y + (24 - Text.LineHeight) / 2f, inRect.width / 2f - 24, 24);

        if( lastScan.hasRimIgnore ){
            GUI.color = Color.green;
        } else {
            if( lastScan.badSize == 0 && lastScan.badFiles == 0 ){
                GUI.color = Color.yellow;
            } else {
                GUI.color = Color.red;
            }
        }

        Widgets.Label(r, "Size".Translate().CapitalizeFirst() + ": " + sizeString);
        GUI.color = Color.white;

        TooltipHandler.TipRegion(r, lastScan.totalSize + " bytes in " + lastScan.totalFiles + " files");
        GenUI.ResetLabelAlign();
    }
}
