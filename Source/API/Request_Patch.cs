using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Verse;

namespace YADA.API.Harmony;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Request_Patch : Request {
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

        Type patchClass = AccessTools.TypeByName(PatchClass);
        if( patchClass is null )
            throw new ArgumentException("Cannot find class " + PatchClass);

        string Owner = "YADA.Repatch";
        new HarmonyLib.Harmony(Owner).CreateClassProcessor(patchClass).Patch();

        return null;
    }
}
