using System;
using System.Xml.Linq;
using Verse;

namespace YADA.API.Map;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Spawn : Request {
    public IntVec3 at;
    public string defName;
    public WipeMode WipeMode = WipeMode.Vanish;

    protected override void processInternal(){
        if( string.IsNullOrEmpty(defName) ){
            throw new ArgumentException("defName is not set");
        }
        GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed(defName), at, Find.CurrentMap, WipeMode);
    }
}
