using HarmonyLib;
using Verse;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using UnityEngine.Rendering;

namespace YADA.API;

class Screenshot : Request {
    protected override void processInternal(){
        FieldInfo fi = AccessTools.Field(typeof(CameraDriver), "cachedCamera");
        Camera c = (Camera) fi.GetValue( Find.CameraDriver );
        if( c == null )
            throw new Exception("no current camera");

        TakeTransparentScreenshot(c, 1024, 768, "/tmp/1.png");
    }

    static RenderTexture renderTexture;

    static void ReadbackCompleted(AsyncGPUReadbackRequest request)
    {
        Log.Warning("[d] ReadbackCompleted");
        // Render texture no longer needed, it has been read back.
//        DestroyImmediate(renderTexture);

        using (var imageBytes = request.GetData<byte>())
        {
            // do something with the pixel data.
        }
    }

    public static void TakeTransparentScreenshot(Camera cam, int width, int height, string fname)
    {
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        Log.Warning("[d] " + renderTexture);
        ScreenCapture.CaptureScreenshotIntoRenderTexture(renderTexture);
        Log.Warning("[d] 1");
        AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGBA32, ReadbackCompleted);
        Log.Warning("[d] 2");
//        ScreenCapture.CaptureScreenshot(fname, 1);
//        var tex = ScreenCapture.CaptureScreenshotAsTexture();
//        Log.Warning("[d] " + tex);
//        TextureAtlasHelper.WriteDebugPNG(tex, fname);
//        UnityEngine.Object.Destroy(tex);
    }
}

