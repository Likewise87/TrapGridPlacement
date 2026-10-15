using HarmonyLib;
using RimWorld;
using Verse;
using TrapGridPlacement;

namespace TrapGridPlacement.Patches
{
    /// <summary>
    /// Trap defs ship without a 2D draw style, so vanilla treats them as click-only.
    /// Assign the same bulk rectangle category used by other filled builds so the
    /// stock DesignationDragger runs (highlight + DesignateMultiCell on mouse-up).
    /// FilledRectangle only so shape keys do not offer oval; we use those keys for trap row layout instead.
    /// </summary>
    [HarmonyPatch(typeof(Designator_Build))]
    [HarmonyPatch(nameof(Designator_Build.DrawStyleCategory), MethodType.Getter)]
    public static class DesignatorBuildTrapDraggablePatch
    {
        private static DrawStyleCategoryDef? cachedRectangleOnly;

        public static void Postfix(Designator_Build __instance, ref DrawStyleCategoryDef __result)
        {
            if (__instance.PlacingDef is not ThingDef thingDef || !TrapGridLayout.UsesGridPlacement(thingDef))
            {
                return;
            }

            cachedRectangleOnly ??= DefDatabase<DrawStyleCategoryDef>.GetNamedSilentFail("FilledRectangle");
            if (cachedRectangleOnly != null)
            {
                __result = cachedRectangleOnly;
            }
        }
    }
}
