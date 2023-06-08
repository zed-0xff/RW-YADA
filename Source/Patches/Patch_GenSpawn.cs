using HarmonyLib;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace YADA;

//static class Patch_GenSpawn {
//    [HarmonyPatch(typeof(GenSpawn), "Spawn")]
//    [HarmonyPatch(new [] {typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool)})]
//    static class Debug_Spawn
//    {
//        static void Postfix(Thing newThing){
//            if( Prefs.DevMode ){
//                Log.Message("[d] Spawn: " + newThing);
//            }
//        }
//    }
//
//    [HarmonyPatch(typeof(GenSpawn), "SpawnBuildingAsPossible")]
//    static class Debug_SpawnBuildingAsPossible
//    {
//        static void Postfix(Building building){
//            if( Prefs.DevMode ){
//                Log.Message("[d] SpawnBuildingAsPossible: " + building);
//            }
//        }
//    }
//}
