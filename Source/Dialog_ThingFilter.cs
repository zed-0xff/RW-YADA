using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
#if RW15
using LudeonTK;
#endif

namespace YADA;

#if RW15
public class Dialog_ThingFilter : Dialog_Rename<Dialog_ThingFilter.FilterRenamable> {
    public Dialog_ThingFilter() : base(new FilterRenamable()) { }

    public sealed class FilterRenamable : IRenameable {
        string _name = "";
        string _renamableLabel = "";

        public string Name { get => _name; set => _name = value; }
        public string RenamableLabel { get => _renamableLabel; set { _renamableLabel = value; _name = value; } }
        public string BaseLabel => "Thing filter";
        public string InspectLabel => Name;
    }

    static IEnumerable<ThingDef> DefsMatching(string filter) =>
        DefDatabase<ThingDef>.AllDefsListForReading
            .Where((ThingDef d) => !d.IsBlueprint && !d.IsFrame && d.defName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);

    IEnumerable<ThingDef> matchingDefs() => DefsMatching(curName);

    public override void OnRenamed(string name) {
        var defs = DefsMatching(name).ToList();
        DebugTools.curTool = new DebugTool("Spawn " + defs.Count + " things here", delegate
        {
            foreach (var d in defs)
                DebugThingPlaceHelper.DebugSpawn(d, UI.MouseCell());
        });
    }

    public override void DoWindowContents(Rect inRect){
        base.DoWindowContents(inRect);

        string text = matchingDefs().Count() + " matching defs";
       
        Widgets.Label(new Rect(15f, 15+35+5, inRect.width - 15f - 15f, 35f), text);
    }

    public override AcceptanceReport NameIsValid(string name) {
        var defs = DefsMatching(name);
        return !string.IsNullOrEmpty(name) && defs.Count() > 0;
    }
}
#else
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
#endif