using HarmonyLib;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Verse;

namespace YADA.API.Harmony;

abstract class HarmonyRequest : Request {
    public HarmonyRequest(){
        fillCache();
    }

    protected class PatchInfo {
        public MethodBase originalMethod;
        public MethodInfo patchMethod;
        public string owner;
        public HarmonyPatchType patchType;

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

        public PatchInfo(MethodBase m, MethodInfo p, string o, HarmonyPatchType t){
            originalMethod = m;
            patchMethod = p;
            owner = o;
            patchType = t;
        }
    }

    protected static Dictionary<int,PatchInfo> patchCache = new Dictionary<int, PatchInfo>();

    protected static void fillCache(){
        if( patchCache.Any() ) return;

        foreach( var m in HarmonyLib.Harmony.GetAllPatchedMethods() ){
            var patches = HarmonyLib.Harmony.GetPatchInfo(m);

            add_list(patches.Prefixes, HarmonyPatchType.Prefix);
            add_list(patches.Postfixes, HarmonyPatchType.Postfix);
            add_list(patches.Transpilers, HarmonyPatchType.Transpiler);
            add_list(patches.Finalizers, HarmonyPatchType.Finalizer);

            void add_list(ReadOnlyCollection<HarmonyLib.Patch> list, HarmonyPatchType type){
                if( list.Any() ){
                    foreach (var patch in list){
                        var p = new PatchInfo(m, patch.PatchMethod, patch.owner, type);
                        patchCache[p.Hash] = p;
                    }
                }
            }
        }
    }
}
