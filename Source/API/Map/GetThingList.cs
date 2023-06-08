using System;
using System.Xml.Linq;
using Verse;

namespace YADA.API.Map;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class GetThingList : Request {
    public IntVec3 at, from, to;

    protected override void processInternal(){
        if( from != null && to != null ){
            var cr = CellRect.FromLimits(from, to);
            foreach (IntVec3 pos in cr){
                addFrom(pos);
            }
        }
        if( at != null ){
            addFrom(at);
        }
    }

    void addFrom(IntVec3 pos){
        foreach( Thing t in pos.GetThingList(Find.CurrentMap) ){
            doc.Add( XElement.Parse(Scribe.saver.DebugOutputFor(t)) );
        }
    }
}
