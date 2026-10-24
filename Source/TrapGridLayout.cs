using RimWorld;
using Verse;

namespace TrapGridPlacement
{
	public enum TrapBandLayoutMode
	{
		/// <summary>Every other row; traps on even relative X (same as first trap row).</summary>
		AlignedRows,

		/// <summary>Every other row; trap X alternates on each trap-bearing relative row.</summary>
		StaggeredRows
	}

	public static class TrapGridLayout
	{
		public static TrapBandLayoutMode Mode { get; private set; }

		public static void CycleMode()
		{
			Mode = Mode == TrapBandLayoutMode.AlignedRows
				? TrapBandLayoutMode.StaggeredRows
				: TrapBandLayoutMode.AlignedRows;
		}

		public static string ModeLabel =>
			Mode == TrapBandLayoutMode.AlignedRows
				? "Aligned trap rows"
				: "Staggered trap rows";

		/// <summary>
		/// Only traps that use vanilla's adjacent-trap ban need the spaced grid.
		/// Other trap defs may legally touch and should keep normal placement.
		/// </summary>
		public static bool UsesGridPlacement(ThingDef thingDef)
		{
			if (thingDef.building is not { isTrap: true } || thingDef.PlaceWorkers == null)
			{
				return false;
			}

			for (int i = 0; i < thingDef.PlaceWorkers.Count; i++)
			{
				if (thingDef.PlaceWorkers[i] is PlaceWorker_NeverAdjacentTrap)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Anchor is the drag rectangle minimum corner (same as designation).
		/// Northernmost row is always a trap band; NW cell (rx==0 on that band) always places.
		/// </summary>
		/// <param name="maxZ">Maximum cell.z in the selection (inclusive), used for row count and parity.</param>
		public static bool CellMatchesLayout(IntVec3 cell, int anchorX, int anchorZ, int maxZ)
		{
			int rx = cell.x - anchorX;
			int rz = cell.z - anchorZ;
			int height = maxZ - anchorZ + 1;
			// Band parity from the north: northernmost row (rz == height-1) is always a trap band.
			int maxTrapRz = height - 1;
			bool isTrapBandRow = ((maxTrapRz - rz) & 1) == 0;
			if (!isTrapBandRow)
			{
				return false;
			}

			if (Mode == TrapBandLayoutMode.AlignedRows)
			{
				return (rx & 1) == 0;
			}

			// Phase 0 on the northernmost trap band so (anchorX, maxZ) always gets a trap.
			int phase = ((maxTrapRz - rz) / 2) & 1;
			return (rx & 1) == phase;
		}
	}
}
