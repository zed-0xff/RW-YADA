using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using System.IO;

namespace YADA;

public static class ScreenshotMaker {
    static Rect shotRect;
    static int queue;
    static bool hideTerrain;
    static int scrCount = -1;

    public static void EnqueueShot(Rect r, bool hideTerrain = false){
        shotRect = r.Rounded();
        ScreenshotMaker.hideTerrain = hideTerrain;
        queue = 2;
        if( hideTerrain ){
            DebugViewSettings.drawTerrain = false;
        }
    }

    public static bool Queued => queue > 0;

    public static void Update(){
        switch( queue ){
            case 0:
                return;
            case 1:
                queue--;
                break;
            case 2:
                queue--;
                return; // wait for next draw cycle
            default:
                queue = 0;
                return;
        }

        try {
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            var tex2 = new Texture2D((int)shotRect.width, (int)shotRect.height, TextureFormat.ARGB32, mipChain: false);
            tex2.SetPixels(tex.GetPixels(
                        (int)shotRect.x,
                        tex.height - (int)shotRect.height - (int)shotRect.y,
                        (int)shotRect.width,
                        (int)shotRect.height));

            string fname = getNextFilename();
            File.WriteAllBytes(fname, tex2.EncodeToPNG());
            Messages.Message(fname + " saved", MessageTypeDefOf.PositiveEvent, historical: false);

            // cleanup
            Object.Destroy(tex);
        } catch( System.Exception ex ){
            Log.Error("[!] " + ex);
        }

        if( hideTerrain ){
            DebugViewSettings.drawTerrain = true;
        }
    }

    static string getNextFilename() {
        string screenshotFolderPath = ModConfig.Settings.textureSavePath;
        DirectoryInfo directoryInfo = new DirectoryInfo(screenshotFolderPath);
        if (!directoryInfo.Exists) {
            directoryInfo.Create();
        }
        string fname;
        do {
            scrCount++;
            fname = screenshotFolderPath + Path.DirectorySeparatorChar.ToString() + string.Format("scr{0:0000}.png", scrCount);
        } while (File.Exists(fname));
        return fname;
    }
}

