using Steamworks;
using System;
using Verse;

namespace YADA.API.Harmony;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Request_UnpatchAll : Request {
    public string Owner;

    // TODO: return not CallResult
    protected override CallResult processInternal(){
        if( string.IsNullOrEmpty(Owner) ){
            throw new ArgumentException("Owner is not set");
        }

        var harmony = new HarmonyLib.Harmony("YADA");
        harmony.UnpatchAll(Owner);

        return null;
    }
}
