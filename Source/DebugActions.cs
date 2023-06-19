using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace YADA;

public static class DebugActions {
    [DebugAction("YADA", "Screenshot..", false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    static void Screenshot(){
        DebugToolsGeneral.GenericRectTool("Screenshot", delegate(CellRect cellRect)
                {
                var uiRect = cellRect.BottomLeft.ToUIRect().Union(cellRect.TopRight.ToUIRect());
                var p1 = UnityEngine.GUIUtility.GUIToScreenPoint(uiRect.min);
                var p2 = UnityEngine.GUIUtility.GUIToScreenPoint(uiRect.max);
                var scrRect = new UnityEngine.Rect(p1, p2-p1);
                ScreenshotMaker.EnqueueShot(scrRect);
                });
    }

    [DebugAction("YADA", "Screenshot w/o terrain..", false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    static void ScreenshotNoTerrain(){
        DebugToolsGeneral.GenericRectTool("Screenshot", delegate(CellRect cellRect)
                {
                var uiRect = cellRect.BottomLeft.ToUIRect().Union(cellRect.TopRight.ToUIRect());
                var p1 = UnityEngine.GUIUtility.GUIToScreenPoint(uiRect.min);
                var p2 = UnityEngine.GUIUtility.GUIToScreenPoint(uiRect.max);
                var scrRect = new UnityEngine.Rect(p1, p2-p1);
                ScreenshotMaker.EnqueueShot(scrRect, hideTerrain: true);
                });
    }

    [DebugAction("YADA", "Move things..", false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    static void MoveThings()
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
    static void SpawnRandomThing()
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
