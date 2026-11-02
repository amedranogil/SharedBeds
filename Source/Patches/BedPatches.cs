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

    /// <summary>
    /// Vanilla ties sleeping slots to the owner list index, which shifts every time someone claims or
    /// releases a shared bed (owners are also sorted by id). A pawn already lying down would then be in
    /// the "wrong" slot and get kicked out by the bed-usable check, so it never sleeps through.
    /// </summary>
    [HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.IsOwner), new[] { typeof(Pawn), typeof(int?) },
        new[] { ArgumentType.Normal, ArgumentType.Out })]
    public static class Building_Bed_IsOwner
    {
        public static void Postfix(Building_Bed __instance, Pawn p, ref int? assignedSleepingSlot)
        {
            if (assignedSleepingSlot.HasValue && __instance.SleepingSlotsCount > 1 && __instance.IsShared())
                assignedSleepingSlot = SharedBedUtility.StableSleepingSlot(__instance, p, assignedSleepingSlot.Value);
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
