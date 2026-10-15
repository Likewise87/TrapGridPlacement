using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using TrapGridPlacement;

namespace TrapGridPlacement.Patches
{
    [HarmonyPatch(typeof(Designator_Place), nameof(Designator_Place.SelectedProcessInput))]
    public static class DesignatorBuildTrapLayoutInputPatch
    {
        public static void Postfix(Designator_Place __instance)
        {
            if (__instance is not Designator_Build)
            {
                return;
            }

            if (__instance.PlacingDef is not ThingDef thingDef || !TrapGridLayout.UsesGridPlacement(thingDef) || thingDef.rotatable)
            {
                return;
            }

            if (!KeyBindingDefOf.Designator_RotateRight.KeyDownEvent && !KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent)
            {
                return;
            }

            TrapGridLayout.CycleMode();
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
            Event.current.Use();
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
            Rect rect = new Rect(leftX, y, 320f, 24f);
            string keys = $"{KeyBindingDefOf.Designator_RotateLeft.MainKeyLabel} / {KeyBindingDefOf.Designator_RotateRight.MainKeyLabel}";
            Widgets.Label(rect, $"{TrapGridLayout.ModeLabel}  ({keys})");
        }
    }
}
