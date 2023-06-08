using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Verse;

namespace YADA.API.Defs;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Patch : Request {
    public object Def;
    public string Field;
    public string Value;
    public bool debug = false;

    protected override void processInternal(){
        if( Def == null)
            throw new ArgumentException("Def is not set");
        var d = Def as Def;
        if( d == null)
            throw new ArgumentException("Def is not a Def");

        if( string.IsNullOrEmpty(Field) )
            throw new ArgumentException("Field is not set");
        if( Value == null )
            throw new ArgumentException("Value is not set");

        Type[] typeArgs = { Def.GetType() };
        var dd = typeof(DefDatabase<>);
        var db = dd.MakeGenericType(typeArgs);

        var m = AccessTools.Method(db, "GetNamed");
        Def def = (Def)m.Invoke(null, new object[]{ d.defName,  true } );
        doc.Add( new XElement("Def", def.ToString()) );

        FieldInfo fi = AccessTools.Field(def.GetType(), Field);
        if( fi == null )
            throw new ArgumentException("no field " + Field + " in " + def);

        var typedValue = ParseHelper.FromString(Value, fi.FieldType);
        // TODO: call setter?
        fi.SetValue(def, typedValue);

        if( debug ){
            doc.Add( new XElement("Value", typedValue?.ToString()) );
        }

        var result = fi.GetValue(def);
        if( result != null ){
            doc.Add(DirectXmlSaver.XElementFromObject(result, result.GetType()));
        }
    }
}
