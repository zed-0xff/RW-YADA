using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Verse;

namespace YADA.API.Map;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class PatchThing : Request {
    public string ThingID;
    public string Field;
    public string Value;

    protected override void processInternal(){
        if( ThingID == null)
            throw new ArgumentException("ThingID is not set");

        Thing thing = findThing(ThingID);
        if( thing == null )
            throw new KeyNotFoundException(ThingID + " not found");

        FieldInfo fi = AccessTools.Field(thing.GetType(), Field);
        if( fi == null )
            throw new ArgumentException("no field " + Field + " in " + thing);

        var typedValue = ParseHelper.FromString(Value, fi.FieldType);
        // TODO: call setter?
        fi.SetValue(thing, typedValue);
    }

    Thing findThing(string id){
        foreach( Thing t in Find.CurrentMap.listerThings.AllThings ){
            if( t.ThingID == id )
                return t;
        }
        return null;
    }
}
