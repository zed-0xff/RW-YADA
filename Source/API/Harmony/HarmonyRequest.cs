using HarmonyLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Verse;

namespace YADA.API.Harmony;

abstract class HarmonyRequest : Request {
    public HarmonyRequest(){
        if( patchCache is null ){
            patchCache = readPatches();
        }
    }

    protected class PatchInfo {
        public MethodBase originalMethod;
        public MethodInfo patchMethod;
        public string owner;
        public HarmonyPatchType patchType;
        public int priority;

        private int hash = -1;

        // not using MethodBase.GetHash() because it changes each app restart
        public int Hash {
            get {
                if( hash == -1 ){
                    hash = Gen.HashCombineInt(0x0b00b135, (int)patchType);
                    hash = Gen.HashCombine(hash, originalMethod.FullDescription());
                    hash = Gen.HashCombine(hash, patchMethod.FullDescription());
                    hash = Gen.HashCombine(hash, owner);
                }
                return hash;
            }
        }

        public XElement to_xml(){
            var x = new XElement("Patch",
                    new XAttribute("PatchMethod", patchMethod.Name),
                    new XAttribute("Owner", owner),
                    new XAttribute("PatchClass", patchMethod.DeclaringType.ToString()),
                    new XAttribute("Hash", Hash.ToString("x8"))
                    );
            if( priority != HarmonyLib.Priority.Normal ){
                x.Add(new XAttribute("Prio", priority));
            }
            return x;
        }

        public void Repatch(HarmonyLib.Harmony harmony){
            switch( patchType ){
                case HarmonyPatchType.Prefix:
                    harmony.Patch(originalMethod, prefix: new HarmonyMethod(patchMethod));
                    break;
                case HarmonyPatchType.Postfix:
                    harmony.Patch(originalMethod, postfix: new HarmonyMethod(patchMethod));
                    break;
                case HarmonyPatchType.Transpiler:
                    harmony.Patch(originalMethod, transpiler: new HarmonyMethod(patchMethod));
                    break;
                case HarmonyPatchType.Finalizer:
                    harmony.Patch(originalMethod, finalizer: new HarmonyMethod(patchMethod));
                    break;
                default:
                    Log.Warning("[?] unknwon patch type: " + patchType);
                    break;
            }
        }
    }

    protected static Dictionary<int,PatchInfo> patchCache = null;

    protected static Dictionary<int,PatchInfo> readPatches(){
        Dictionary<int,PatchInfo> result = new Dictionary<int,PatchInfo>();

        foreach( var m in HarmonyLib.Harmony.GetAllPatchedMethods() ){
            var patches = HarmonyLib.Harmony.GetPatchInfo(m);

            add_list(patches.Prefixes, HarmonyPatchType.Prefix);
            add_list(patches.Postfixes, HarmonyPatchType.Postfix);
            add_list(patches.Transpilers, HarmonyPatchType.Transpiler);
            add_list(patches.Finalizers, HarmonyPatchType.Finalizer);

            void add_list(ReadOnlyCollection<HarmonyLib.Patch> list, HarmonyPatchType type){
                if( list.Any() ){
                    foreach (var patch in list){
                        var p = new PatchInfo();

                        p.originalMethod = m;
                        p.patchMethod = patch.PatchMethod;
                        p.owner = patch.owner;
                        p.patchType = type;
                        p.priority = patch.priority;

                        result[p.Hash] = p;
                    }
                }
            }
        }
        return result;
    }
}
