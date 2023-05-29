using Steamworks;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Xml.Linq;
using Verse;

namespace YADA.API.Harmony;

class Request_GetAllPatchedMethods : Request {
    // TODO: return not CallResult
    protected override CallResult processInternal(){
        var originalMethods = HarmonyLib.Harmony.GetAllPatchedMethods()
            .ToList()
            .OrderBy(m => m.DeclaringType.ToString())
            .ThenBy(m => m.Name);

        foreach( var m in originalMethods ){
            var x_method = new XElement("Method", new XAttribute("Class", m.DeclaringType.ToString()), new XAttribute("Name", m.Name));
            doc.Add(x_method);
            var patches = HarmonyLib.Harmony.GetPatchInfo(m);
            add_list(x_method, patches.Prefixes, "Prefix");
            add_list(x_method, patches.Postfixes, "Postfix");
            add_list(x_method, patches.Transpilers, "Transpiler");
            add_list(x_method, patches.Finalizers, "Finalizer");
        }
        return null;
    }

    void add_list(XElement x_parent, ReadOnlyCollection<HarmonyLib.Patch> list, string name){
        if( list.Any() ){
            foreach (var patch in list){
                x_parent.Add(new XElement("Patch",
                            new XAttribute("PatchMethod", name),
                            new XAttribute("Owner", patch.owner),
                            new XAttribute("PatchClass", patch.PatchMethod.DeclaringType.ToString())
                            ));
            }
        }
    }
}
