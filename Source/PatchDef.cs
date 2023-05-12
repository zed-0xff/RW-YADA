using Verse;

namespace zed_0xff.YADA;

// Field 'PatchDef.className' is never assigned to, and will always have its default value null
#pragma warning disable CS0649

public class PatchDef : Def {

    string className;

    public override void PostLoad() {
        base.PostLoad();
        Log.Warning("[d] PatchDef.PostLoad " + this + " " + className);
    }
}

#pragma warning restore CS0649
