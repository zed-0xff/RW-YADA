using HarmonyLib;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Verse;

namespace YADA.API.Defs;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Get : Request {
    public Type defType;
    public string defName;

    public object Def;
    public bool debug = false;

    protected override void processInternal(){
//        if( Def == null)
//            throw new ArgumentException("Def is not set");
//        var d = Def as Def;
//        if( d == null)
//            throw new ArgumentException("Def is not a Def");

        if( defType == null )
            throw new ArgumentException("defType is not set");
        if( defName == null )
            throw new ArgumentException("defName is not set");

        Type[] typeArgs = { defType };
        var dd = typeof(DefDatabase<>);
        var db = dd.MakeGenericType(typeArgs);

        var m = AccessTools.Method(db, "GetNamed");
        Def def = (Def)m.Invoke(null, new object[]{ defName,  true } );

        doc.Add(serialize(def, def.GetType().Name, false));
    }

    void addField(XElement xobj, object value, string tagName, bool addDefault = false){
        if( value.GetType().IsValueType ){
            if( !addDefault && (value == null || value == Activator.CreateInstance(value.GetType())) ){
                // is default
                return;
            }
            xobj.Add( new XElement(tagName, value) );
            return;
        }
        if( value is string ){
            if( value as string != "" )
                xobj.Add( new XElement(tagName, value) );
            return;
        }

        if( value is Type ){
            xobj.Add( new XElement(tagName, value) );
            return;
        }

        if( value is IList ){
            var l = value as IEnumerable;
            var xlist = new XElement(tagName);
            bool was = false;
            foreach( var x in l ){
                addField(xlist, x, "li");
                was = true;
            }
            if( was )
                xobj.Add(xlist);
            return;
        }

        if( value is Def d){
            xobj.Add( new XElement(tagName, new XAttribute("Class", value.GetType()), d.defName ));
            return;
        }

        xobj.Add( serialize(value, tagName) );
    }

    XElement serialize(object obj, string tagName, bool addClass = true){
        var xobj = new XElement(tagName);
        if( addClass )
            xobj.Add( new XAttribute("Class", obj.GetType()) );

        var fields = obj.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public)
            .OrderBy(fi => fi.Name);

        object defObj = null;
        if( obj is Def ){
            defObj = Activator.CreateInstance(obj.GetType());
        }

        foreach( var fi in fields ){
            UnsavedAttribute unsavedAttribute = fi.TryGetAttribute<UnsavedAttribute>();
            if (unsavedAttribute != null && !unsavedAttribute.allowLoading){
                continue;
            }

            var value = fi.GetValue(obj);
            if( value == null )
                continue;

            if( defObj != null && value == fi.GetValue(defObj))
                continue;

            addField(xobj, value, fi.Name);
        }
        return xobj;
    }
}
