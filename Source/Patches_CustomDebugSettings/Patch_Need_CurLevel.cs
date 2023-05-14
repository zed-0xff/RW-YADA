using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;

namespace zed_0xff.YADA; 

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class Patch_Need_CurLevel {
    [HarmonyPatch(typeof(Need), "CurLevel", MethodType.Getter)]
    public static class Patch_Getter {
        /// <summary>
        /// Returns the maximum value whenever needs are checked <br />
        /// </summary>
        /// <remarks>
        /// Won't work if a Need implements the getter without calling base.CurLevel, but no non-mod Needs currently do that
        /// </remarks>
        public static void Postfix(ref float __result, Need __instance) {
            if (Yada_DebugSettings.freezeNeedsAtMaximum) {
                __result = __instance.MaxLevel;
            }
        }
    }
}
