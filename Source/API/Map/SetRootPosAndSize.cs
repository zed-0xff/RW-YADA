using RimWorld;
using Verse;
using UnityEngine;

namespace YADA.API.Map;

// Field '...' is never assigned to / never used
#pragma warning disable CS0649, CS0169, CS0414

class SetRootPosAndSize : Request {
    public Vector3 rootPos = InvalidPos;
    public float rootSize;

    static readonly Vector3 InvalidPos = new Vector3(-1, -1, -1);

    protected override void processInternal(){
        if( rootPos == InvalidPos )
            rootPos = Find.CurrentMap.rememberedCameraPos.rootPos;

        if( rootSize == 0 )
            rootSize = Find.CurrentMap.rememberedCameraPos.rootSize;

        Find.CameraDriver?.SetRootPosAndSize(rootPos, rootSize);
    }
}

