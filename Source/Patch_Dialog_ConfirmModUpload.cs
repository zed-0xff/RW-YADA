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
        public int ignoredFiles;
        public long ignoredSize;
        public int totalFiles;
        public long totalSize;
        public int badFiles;
        public long badSize;

        public static ScanResult operator +(ScanResult a, ScanResult b){
            ScanResult sr = new ScanResult();
            sr.hasRimIgnore = a.hasRimIgnore || b.hasRimIgnore;
            sr.ignoredFiles = a.ignoredFiles + b.ignoredFiles;
            sr.ignoredSize = a.ignoredSize + b.ignoredSize;
            sr.totalFiles = a.totalFiles + b.totalFiles;
            sr.totalSize = a.totalSize + b.totalSize;
            sr.badFiles = a.badFiles + b.badFiles;
            sr.badSize = a.badSize + b.badSize;
            return sr;
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

    private static ScanResult sr;

    static void Prefix(Rect inRect, ref ModMetaData ___mod, ref Dialog_ConfirmModUpload __instance){
        sr = new ScanResult();
        Scanner.ScanDir(___mod.RootDir, delegate(FileSystemInfo fsi){
                if( fsi is FileInfo fi) {
                  sr.totalFiles++;
                  sr.totalSize += fi.Length;
                }
                });
        sr.hasRimIgnore = File.Exists(Path.Combine(___mod.RootDir.ToString(), Scanner.rimignoreFname));
        if( !sr.hasRimIgnore ){
            __instance.buttonCText = ("Add " + Scanner.rimignoreFname).Colorize(new Color(0,0.8f,0));
        }
    }

    static void Postfix(Rect inRect, ref ModMetaData ___mod){
        Vector2 topLeft = new Vector2(inRect.x + 10f, inRect.height - 35 - 24 - 10);
        GenUI.SetLabelAlign(TextAnchor.MiddleRight);

        string sizeString = formatSize(sr.totalSize);
        var r = new Rect(topLeft.x + inRect.width / 2, topLeft.y + (24 - Text.LineHeight) / 2f, inRect.width / 2f - 24, 24);

        if( sr.hasRimIgnore ){
            GUI.color = Color.green;
        } else {
            if( sr.badSize == 0 && sr.badFiles == 0 ){
                GUI.color = Color.yellow;
            } else {
                GUI.color = Color.red;
            }
        }

        Widgets.Label(r, "Size".Translate().CapitalizeFirst() + ": " + sizeString);
        GUI.color = Color.white;

        TooltipHandler.TipRegion(r, sr.totalSize + " bytes in " + sr.totalFiles + " files");
        GenUI.ResetLabelAlign();

//        List<FloatMenuOption> list = new List<FloatMenuOption>();
//        list.Add( new FloatMenuOption("foo", delegate{}) );
//        Find.WindowStack.Add(new FloatMenu(list));
    }
}
