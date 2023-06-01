using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Verse;

namespace YADA.API.Harmony;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Request_Repatch : HarmonyRequest {
    public List<string> Hashes;

    protected override void processInternal(){
        foreach( string Hash in Hashes ){
            PatchInfo patch = patchCache[Convert.ToInt32(Hash, 16)];

            var harmony = new HarmonyLib.Harmony(patch.owner);
            patch.Repatch(harmony);
        }
    }
}
