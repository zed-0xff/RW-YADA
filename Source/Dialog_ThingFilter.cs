using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YADA;

public class Dialog_ThingFilter : Dialog_Rename {
    public Dialog_ThingFilter() {
    }

    IEnumerable<ThingDef> matchingDefs(){
        return DefDatabase<ThingDef>
            .AllDefsListForReading
            .Where((ThingDef d) => !d.IsBlueprint && !d.IsFrame && d.defName.IndexOf(curName, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    public override void DoWindowContents(Rect inRect){
        base.DoWindowContents(inRect);

        string text = matchingDefs().Count() + " matching defs";
       
        Widgets.Label(new Rect(15f, 15+35+5, inRect.width - 15f - 15f, 35f), text);
    }

    public override AcceptanceReport NameIsValid(string name) {
        var defs = matchingDefs();
        return !string.IsNullOrEmpty(name) && defs.Count() > 0;
    }

    public override void SetName(string name) {
        var defs = matchingDefs();

        DebugTools.curTool = new DebugTool("Spawn " + defs.Count() + " things here", delegate
                {
                foreach( var d in defs ){
                    DebugThingPlaceHelper.DebugSpawn(d, UI.MouseCell());
                }
                });
    }
}
