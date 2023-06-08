using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System;
using Verse;

namespace YADA.API.Harmony;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class GetAllPatchedMethods : HarmonyRequest {
    public bool returnCached;

    protected override void processInternal(){
        var patches = (returnCached ? patchCache : readPatches()).Values
            .OrderBy(p => p.originalMethod.DeclaringType.ToString())
            .ThenBy(p => p.originalMethod.Name);

        XElement x_method = null;
        MethodBase m = null;
        foreach( var p in patches ){
            if( m != p.originalMethod ){
                m = p.originalMethod;
                x_method = new XElement("Method", new XAttribute("Class", m.DeclaringType.ToString()), new XAttribute("Name", m.Name));
                doc.Add(x_method);
            }

            x_method.Add(p.to_xml());
        }
    }
}
