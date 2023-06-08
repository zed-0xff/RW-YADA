using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Verse;

namespace YADA.API;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Call : Request {
    public string fqmn;
    public bool stringify = true;
    public bool cacheCompiled = true;
    public bool debug = false;

    static Dictionary<string, Utils.DynInvoker> cache = new Dictionary<string, Utils.DynInvoker>();

    protected override void processInternal(){
        if( string.IsNullOrEmpty(fqmn) ){
            throw new ArgumentException("fqmn is not set");
        }

        Utils.DynInvoker invoker = null;
        if(!cacheCompiled || !cache.TryGetValue(fqmn, out invoker)) {
            invoker = Utils.FastCallAny(fqmn, debug: debug);
            if( cacheCompiled ){
                cache[fqmn] = invoker;
            }
        }

//        if( debug ){
//            // show opcodes instead of calling them
//            StringBuilder sb = new StringBuilder();
//            foreach( var x in invoker.GetInvocationList() ){
//                Utils.Disasm(x.Method, sb);
//                sb.AppendLine();
//            }
//            doc.Add( sb.ToString() );
//            return;
//        }

        object result = invoker.Invoke();

        if( result != null ){
            if( stringify ){
                doc.Add( result.ToString() );
            } else {
                doc.Add(DirectXmlSaver.XElementFromObject(result, result.GetType()));
            }
        }
    }
}
