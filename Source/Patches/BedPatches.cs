using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SharedBeds.Patches
{
    [HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.DrawColorTwo), MethodType.Getter)]
    public static class Building_Bed_DrawColorTwo
    {
        public static void Postfix(Building_Bed __instance, ref Color __result)
        {
            if (__instance.def.building != null && !__instance.def.building.bed_UseSheetColor)
                return;
            if (__instance.IsShared())
                __result = CompSharedBed.SheetColorShared;
        }
    }

    /// <summary>Medical / prisoner / slave modes replace shared mode.</summary>
    [HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.Medical), MethodType.Setter)]
    public static class Building_Bed_Medical
    {
        public static void Postfix(Building_Bed __instance) => __instance.GetComp<CompSharedBed>()?.Notify_BedModeChanged();
    }

    [HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.ForOwnerType), MethodType.Setter)]
    public static class Building_Bed_ForOwnerType
    {
        public static void Postfix(Building_Bed __instance) => __instance.GetComp<CompSharedBed>()?.Notify_BedModeChanged();
    }

    [HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.ForPrisoners), MethodType.Setter)]
    public static class Building_Bed_ForPrisoners
    {
        public static void Postfix(Building_Bed __instance) => __instance.GetComp<CompSharedBed>()?.Notify_BedModeChanged();
    }

    /// <summary>Show "Shared" instead of "Unowned" over free shared beds.</summary>
    [HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.DrawGUIOverlay))]
    public static class Building_Bed_DrawGUIOverlay
    {
        public static bool Prefix(Building_Bed __instance)
        {
            if (__instance.OwnersForReading.Count > 0 || !__instance.IsShared())
                return true;
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
                GenMapUI.DrawThingLabel(__instance, "SB_SharedLabel".Translate(), GenMapUI.DefaultThingLabelColor);
            return false;
        }
    }

    /// <summary>Shared beds can't be assigned to a pawn.</summary>
    [HarmonyPatch(typeof(CompAssignableToPawn_Bed), "ShouldShowAssignmentGizmo")]
    public static class CompAssignableToPawn_Bed_ShouldShowAssignmentGizmo
    {
        public static void Postfix(CompAssignableToPawn_Bed __instance, ref bool __result)
        {
            if (__result && ((Building_Bed)__instance.parent).IsShared())
                __result = false;
        }
    }
}
