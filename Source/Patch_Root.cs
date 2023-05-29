using HarmonyLib;
using Verse;

namespace YADA;

[HarmonyPatch(typeof(Root), nameof(Root.OnGUI))]
static class Patch__Root__OnGUI {
    static void Prefix(){
        LogOverlay.drawn = false;
    }
    static void Postfix(){
        if( !LogOverlay.drawn ){
            LogOverlay.Draw();
        }
    }
}
