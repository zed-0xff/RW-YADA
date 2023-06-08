using System;
using System.Text;
using System.Xml.Linq;

namespace YADA.API;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Disasm : Request {
    public string fqmn;
    public bool original;

    protected override void processInternal(){
        var mi = Utils.fqmnToMethodInfo(fqmn, out string error);
        if( !string.IsNullOrEmpty(error) )
            throw new ArgumentException(error);

        if( mi == null )
            throw new ArgumentException("no such method: " + fqmn);

        StringBuilder sb = new StringBuilder();
        Utils.Disasm(mi, sb, original: original);
        sb.AppendLine();
        doc.Add( sb.ToString() );
    }
}
