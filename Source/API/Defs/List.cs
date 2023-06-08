using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Verse;

namespace YADA.API.Defs;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class List : Request {
    public object def;
    public bool skipFrames = true;
    public bool skipBlueprints = true;

    protected override void processInternal(){
        if( def == null ){
            // list DBs
            foreach (Type t in typeof(Def).AllSubclassesNonAbstract()){
                doc.Add( new XElement("li", t.ToString()) );
            }
        } else {
            Type[] typeArgs = { def.GetType() };
            var dd = typeof(DefDatabase<>);
            var db = dd.MakeGenericType(typeArgs);
            var m = AccessTools.Method(db, "get_AllDefs");
            var defs = (IEnumerable<Def>)m.Invoke(null, null);

            var container = new XElement("Defs");
            doc.Add(container);
            foreach( Def d in defs ){
                if( d is ThingDef td ){
                    if( skipFrames && td.IsFrame )
                        continue;
                    if( skipBlueprints && td.IsBlueprint )
                        continue;
                }
                var tag = d.GetType().ToString();
                if( tag.StartsWith("RimWorld.") )
                    tag = tag.Replace("RimWorld.", "");
                if( tag.StartsWith("Verse.") )
                    tag = tag.Replace("Verse.", "");
                container.Add( new XElement(tag, new XElement("defName", d.defName)) );
            }
        }
    }
}
