using HarmonyLib;
using Verse;

namespace TrapGridPlacement
{
    [StaticConstructorOnStartup]
    public static class TrapGridPlacementMod
    {
        static TrapGridPlacementMod()
        {
            try
            {
                var harmony = new Harmony("TSA.TrapGridPlacement");
                harmony.PatchAll();
                Log.Message("[TSA.TrapGridPlacement] Harmony patches initialized.");
            }
            catch (System.Exception ex)
            {
                Log.Error($"[TSA.TrapGridPlacement] Harmony initialization failed: {ex}");
            }
        }
    }
}
