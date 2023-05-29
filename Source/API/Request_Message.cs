using Steamworks;
using RimWorld;
using Verse;

namespace YADA.API;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class Request_Message : Request {
    public string Text;
    public bool historical;

    protected override CallResult processInternal(){
        Messages.Message( Text, MessageTypeDefOf.SilentInput, historical );
        return null;
    }
}