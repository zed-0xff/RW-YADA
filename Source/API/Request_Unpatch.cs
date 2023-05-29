using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Verse;

namespace YADA.API.Harmony;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Request_Unpatch : Request {
    public string Class;
    public string Method;
    public string PatchClass;

    // TODO: return not CallResult
    protected override CallResult processInternal(){
        if( string.IsNullOrEmpty(Class) )
            throw new ArgumentException("Class is not set");
        if( string.IsNullOrEmpty(Method) )
            throw new ArgumentException("Method is not set");
        if( string.IsNullOrEmpty(PatchClass) )
            throw new ArgumentException("PatchClass is not set");

        foreach( var original in HarmonyLib.Harmony.GetAllPatchedMethods()){
            var patches = HarmonyLib.Harmony.GetPatchInfo(original);
            if (patches is null) continue;

            if( original.DeclaringType.ToString() != Class || original.Name != Method )
                continue;

            var harmony = new HarmonyLib.Harmony("YADA");

            unpatch(patches.Prefixes);
            unpatch(patches.Postfixes);
            unpatch(patches.Transpilers);
            unpatch(patches.Finalizers);
            return null;

            void unpatch(ReadOnlyCollection<HarmonyLib.Patch> coll){
                if( coll is null ) return;

                foreach (var patch in coll){
                    if( patch.PatchMethod.DeclaringType.ToString() == PatchClass ){
                        harmony.Unpatch(original, patch.PatchMethod);
                    }
                }
            }
        }

        throw new KeyNotFoundException("patch not found");
    }
}
