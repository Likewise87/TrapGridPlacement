using System.Collections.Generic;
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
		static readonly AccessTools.FieldRef<Designator_Place, Rot4> PlacingRotRef =
			AccessTools.FieldRefAccess<Designator_Place, Rot4>("placingRot");

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

			int anchorX = dragCells[0].x;
			int anchorZ = dragCells[0].z;
			int maxZ = dragCells[0].z;
			for (int i = 1; i < dragCells.Count; i++)
			{
				IntVec3 c = dragCells[i];
				if (c.x < anchorX) anchorX = c.x;
				if (c.z < anchorZ) anchorZ = c.z;
				if (c.z > maxZ) maxZ = c.z;
			}

			Rot4 rot = PlacingRotRef(__instance);

			for (int i = 0; i < dragCells.Count; i++)
			{
				IntVec3 cell = dragCells[i];
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
