using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using TrapGridPlacement;

namespace TrapGridPlacement.Patches
{
    [HarmonyPatch]
    public static class DesignatorMultiCellPatch
    {
        private static bool loggedTrapIntercept;

        public static MethodBase? TargetMethod()
        {
            MethodBase? method = AccessTools.Method(typeof(Designator), nameof(Designator.DesignateMultiCell), new[] { typeof(IEnumerable) });
            if (method != null)
            {
                return method;
            }

            // Some references expose the generic enumerable signature.
            return AccessTools.Method(typeof(Designator), nameof(Designator.DesignateMultiCell), new[] { typeof(IEnumerable<IntVec3>) });
        }

        public static bool Prefix(Designator __instance, object? cells)
        {
            if (__instance is not Designator_Build designatorBuild)
            {
                return true;
            }

            if (designatorBuild.PlacingDef is not ThingDef thingDef || !TrapGridLayout.UsesGridPlacement(thingDef))
            {
                return true;
            }

            if (cells is not IEnumerable enumerableCells)
            {
                return true;
            }

            var cellList = enumerableCells.Cast<object>().OfType<IntVec3>().ToList();
            if (cellList.Count == 0)
            {
                __instance.Finalize(somethingSucceeded: false);
                return false;
            }

            if (!loggedTrapIntercept)
            {
                loggedTrapIntercept = true;
                Log.Message($"[TSA.TrapGridPlacement] Trap drag interception active for {thingDef.defName}.");
            }

            if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(__instance.TutorTagDesignate, cellList)))
            {
                return false;
            }

            int anchorX = cellList.Min(c => c.x);
            int anchorZ = cellList.Min(c => c.z);
            int maxZ = cellList.Max(c => c.z);
            bool somethingSucceeded = false;
            bool warningShown = false;

            foreach (IntVec3 cell in cellList)
            {
                if (!TrapGridLayout.CellMatchesLayout(cell, anchorX, anchorZ, maxZ))
                {
                    continue;
                }

                if (!__instance.CanDesignateCell(cell).Accepted)
                {
                    continue;
                }

                __instance.DesignateSingleCell(cell);
                somethingSucceeded = true;
                if (!warningShown)
                {
                    warningShown = __instance.ShowWarningForCell(cell);
                }
            }

            __instance.Finalize(somethingSucceeded);

            if (TutorSystem.TutorialMode)
            {
                TutorSystem.Notify_Event(new EventPack(__instance.TutorTagDesignate, cellList));
            }

            return false;
        }
    }
}
