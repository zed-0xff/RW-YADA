using HarmonyLib;
using RimWorld;

namespace zed_0xff.YADA; 
[HarmonyPatch(typeof(AutoUndrafter), "ShouldAutoUndraft")]
public class Patch_AutoUndrafter_ShouldAutoUndraft {
    /// <summary>
    /// Prevents ShouldAutoUndraft from returning true <br />
    /// </summary>
    /// <remarks>
    /// Called once every hundred ticks for every drafted pawn (via <see cref="Pawn_DraftController" />)
    /// </remarks>
    public static void Postfix(ref bool __result) {
        if (Yada_DebugSettings.disableAutomaticUndraft) {
            __result = false;
        }
    }
}