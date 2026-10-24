using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using TrapGridPlacement;

namespace TrapGridPlacement.Patches
{
	/// <summary>
	/// Prefix so draw-style next/prev cycle trap layout instead of vanilla's empty FilledRectangle list.
	/// Rotate keys remain a secondary alias.
	/// </summary>
	[HarmonyPatch(typeof(Designator_Place), nameof(Designator_Place.SelectedProcessInput))]
	public static class DesignatorBuildTrapLayoutInputPatch
	{
		public static bool Prefix(Designator_Place __instance)
		{
			if (__instance is not Designator_Build)
			{
				return true;
			}

			if (__instance.PlacingDef is not ThingDef thingDef || !TrapGridLayout.UsesGridPlacement(thingDef) || thingDef.rotatable)
			{
				return true;
			}

			bool drawStyle =
				KeyBindingDefOf.Designator_NextDrawStyle.KeyDownEvent
				|| KeyBindingDefOf.Designator_PreviousDrawStyle.KeyDownEvent;
			bool rotate =
				KeyBindingDefOf.Designator_RotateRight.KeyDownEvent
				|| KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent;
			if (!drawStyle && !rotate)
			{
				return true;
			}

			TrapGridLayout.CycleMode();
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
			Event.current.Use();
			return false;
		}
	}

	[HarmonyPatch(typeof(Designator_Place), nameof(Designator_Place.DoExtraGuiControls))]
	public static class DesignatorBuildTrapLayoutGuiPatch
	{
		public static void Postfix(Designator_Place __instance, float leftX, float bottomY)
		{
			if (__instance is not Designator_Build)
			{
				return;
			}

			if (__instance.PlacingDef is not ThingDef thingDef || !TrapGridLayout.UsesGridPlacement(thingDef) || thingDef.rotatable)
			{
				return;
			}

			float y = bottomY - 30f;
			Rect rect = new Rect(leftX, y, 360f, 24f);
			string keys =
				$"{KeyBindingDefOf.Designator_PreviousDrawStyle.MainKeyLabel} / {KeyBindingDefOf.Designator_NextDrawStyle.MainKeyLabel}";
			Widgets.Label(rect, $"{TrapGridLayout.ModeLabel}  ({keys})");
		}
	}
}
