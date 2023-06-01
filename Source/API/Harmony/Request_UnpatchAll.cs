using System;
using Verse;

namespace YADA.API.Harmony;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Request_UnpatchAll : HarmonyRequest {
    public string Owner;

    protected override void processInternal(){
        if( string.IsNullOrEmpty(Owner) ){
            throw new ArgumentException("Owner is not set");
        }

        var harmony = new HarmonyLib.Harmony(Owner);
        harmony.UnpatchAll(Owner);
    }
}
