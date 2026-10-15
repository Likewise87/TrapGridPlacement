using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using TrapGridPlacement;

namespace TrapGridPlacement.Patches
{
    [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.RenderHighlight))]
    public static class DesignatorBuildTrapGhostHighlightPatch
    {
        public static bool Prefix(Designator_Build __instance, List<IntVec3> dragCells)
        {
            if (__instance.PlacingDef is not ThingDef thingDef || !TrapGridLayout.UsesGridPlacement(thingDef))
            {
                return true;
            }

            if (dragCells == null || dragCells.Count == 0)
            {
                return true;
            }

            int anchorX = dragCells.Min(c => c.x);
            int anchorZ = dragCells.Min(c => c.z);
            int maxZ = dragCells.Max(c => c.z);
            Rot4 rot = Traverse.Create(__instance).Field<Rot4>("placingRot").Value;

            foreach (IntVec3 cell in dragCells)
            {
                if (!TrapGridLayout.CellMatchesLayout(cell, anchorX, anchorZ, maxZ))
                {
                    continue;
                }

                Color ghostCol = __instance.CanDesignateCell(cell).Accepted
                    ? Designator_Place.CanPlaceColor
                    : Designator_Place.CannotPlaceColor;
                GhostDrawer.DrawGhostThing(cell, rot, thingDef, null, ghostCol, AltitudeLayer.Blueprint);
            }

            return false;
        }
    }
}
