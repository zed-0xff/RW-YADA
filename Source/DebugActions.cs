using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace YADA;

public static class DebugActions {

    [DebugAction("YADA", "Move things..", false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, requiresBiotech = true)]
    private static void CreateBabyFromParents()
    {
        DebugTool tool = null;
        IEnumerable<Thing> things = null;
        tool = new DebugTool("Move from...", delegate
                {
                things = ThingsAt(UI.MouseCell());
                if (things != null && things.Count() != 0){
                    DebugTools.curTool = new DebugTool("Move to...", delegate
                            {
                            foreach( Thing t in things ){
                                t.Position = UI.MouseCell();
                            }
                            DebugTools.curTool = tool;
                            });
                }
                });
        DebugTools.curTool = tool;
        static IEnumerable<Thing> ThingsAt(IntVec3 c) {
            return Find.CurrentMap.thingGrid.ThingsAt(c);
        }
    }
}
