using HarmonyLib;
using System;
using System.Reflection;
using System.Xml.Linq;

namespace YADA.API.Harmony;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Request_RepatchAll : HarmonyRequest {
    public string Owner;

    protected override void processInternal(){
        if( string.IsNullOrEmpty(Owner) ){
            throw new ArgumentException("Owner is not set");
        }

        var harmony = new HarmonyLib.Harmony(Owner);
        harmony.UnpatchAll(Owner);

        foreach( var patch in patchCache.Values ){
            if( patch.owner == Owner ){
                try {
                    patch.Repatch(harmony);
                } catch( Exception ex ) {
                    doc.Add( new XElement("Exception",
                                new XElement("Message", ex.Message),
                                new XElement("Method", patch.originalMethod.FullDescription()),
                                patch.to_xml()
                                ));
                }
            }
        }
    }
}
