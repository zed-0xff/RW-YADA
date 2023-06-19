using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Verse;

namespace YADA.API;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Eval : Request {
    public string expression;
    public bool stringify = true;
    public bool debug = false;

    protected override void processInternal(){
        if( string.IsNullOrEmpty(expression) ){
            throw new ArgumentException("expression is not set");
        }

        var invoker = ExpCompiler.Compile(expression, null);
        object result = invoker.Invoke(null);

        if( result != null ){
            if( stringify ){
                doc.Add( result.ToString() );
            } else {
                doc.Add(DirectXmlSaver.XElementFromObject(result, result.GetType()));
            }
        }
    }
}
