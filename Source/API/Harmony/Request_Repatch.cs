using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Verse;

namespace YADA.API.Harmony;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Request_Repatch : HarmonyRequest {
    public string Hash;

    protected override void processInternal(){
        PatchInfo patch = patchCache[Convert.ToInt32(Hash, 16)];

        var harmony = new HarmonyLib.Harmony(patch.owner);

        switch( patch.patchType ){
            case HarmonyPatchType.Prefix:
                harmony.Patch(patch.originalMethod, prefix: new HarmonyMethod(patch.patchMethod));
                break;
            case HarmonyPatchType.Postfix:
                harmony.Patch(patch.originalMethod, postfix: new HarmonyMethod(patch.patchMethod));
                break;
            case HarmonyPatchType.Transpiler:
                harmony.Patch(patch.originalMethod, transpiler: new HarmonyMethod(patch.patchMethod));
                break;
            case HarmonyPatchType.Finalizer:
                harmony.Patch(patch.originalMethod, finalizer: new HarmonyMethod(patch.patchMethod));
                break;
            default:
                Log.Warning("[?] unknwon patch type: " + patch.patchType);
                break;
        }
    }
}
