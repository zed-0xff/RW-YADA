using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace zed_0xff.YADA;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Match RimWorld naming conventions")]
[SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer", Justification = "Clear defaults are important here")]
public class Yada_DebugSettings {
    /// <summary>
    /// Freezes pawn needs at maximum
    /// </summary>
    /// <remarks>
    /// See <see cref="Patch_Need_CurLevel.Patch_Getter.Postfix">Patch_Need_CurLevel</see> for implementation details
    /// </remarks>
    public static bool freezeNeedsAtMaximum = false;
}
