using System.Collections;
using System.Collections.Generic;
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

			var cellList = new List<IntVec3>();
			foreach (object? item in enumerableCells)
			{
				if (item is IntVec3 cell)
				{
					cellList.Add(cell);
				}
			}

			if (cellList.Count == 0)
			{
				__instance.Finalize(somethingSucceeded: false);
				return false;
			}

			if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(__instance.TutorTagDesignate, cellList)))
			{
				return false;
			}

			int anchorX = cellList[0].x;
			int anchorZ = cellList[0].z;
			int maxZ = cellList[0].z;
			for (int i = 1; i < cellList.Count; i++)
			{
				IntVec3 c = cellList[i];
				if (c.x < anchorX) anchorX = c.x;
				if (c.z < anchorZ) anchorZ = c.z;
				if (c.z > maxZ) maxZ = c.z;
			}

			bool somethingSucceeded = false;
			bool warningShown = false;

			for (int i = 0; i < cellList.Count; i++)
			{
				IntVec3 cell = cellList[i];
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
