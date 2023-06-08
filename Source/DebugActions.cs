using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace YADA;

public static class DebugActions {

    [DebugAction("YADA", "Move things..", false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
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

    [DebugAction("YADA", "Spawn random thing", false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void SpawnRandomThing()
    {
        var variants = (from x in DebugThingPlaceHelper.TryPlaceOptionsForStackCount(1, direct: false) select x).ToList();
        Random rand = new Random();
        int i = rand.Next(0, variants.Count);
        variants[i].action();
    }

    [DebugAction("YADA", "Spawn filtered things...", false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void SpawnFilteredThings()
    {
        Find.WindowStack.Add(new Dialog_ThingFilter());
    }
}
