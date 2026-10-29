using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SharedBeds.Patches
{
    /// <summary>
    /// Bed choice for colonists without a permanent bed:
    /// vanilla (own bed / lover's bed / medical) first, then lover's shared bed, then best free shared bed.
    /// </summary>
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.FindBedFor),
        new[] { typeof(Pawn), typeof(Pawn), typeof(bool), typeof(bool), typeof(GuestStatus?) })]
    public static class RestUtility_FindBedFor
    {
        public static void Postfix(ref Building_Bed __result, Pawn sleeper, Pawn traveler, bool checkSocialProperness,
            bool ignoreOtherReservations, GuestStatus? guestStatus)
        {
            if (!SharedBedUtility.CanUseSharedBeds(sleeper, guestStatus))
                return;
            if (__result != null)
            {
                if (__result.Medical || __result.IsOwner(sleeper))
                    return;
                if (!__result.IsShared())
                {
                    // A lover's personal bed, or an unowned normal bed when the player prefers those.
                    if (__result.OwnersForReading.Count > 0 || !SharedBedsMod.Settings.preferSharedOverUnowned)
                        return;
                }
            }
            Building_Bed shared = SharedBedUtility.FindSharedBedFor(sleeper, traveler, checkSocialProperness, ignoreOtherReservations);
            if (shared != null)
                __result = shared;
        }
    }

    /// <summary>Claims of shared beds are temporary and never replace a permanent bed.</summary>
    [HarmonyPatch(typeof(Pawn_Ownership), nameof(Pawn_Ownership.ClaimBedIfNonMedical))]
    public static class Pawn_Ownership_ClaimBedIfNonMedical
    {
        public static bool Prefix(Pawn_Ownership __instance, Building_Bed newBed, Pawn ___pawn, ref bool __result)
        {
            if (!newBed.IsShared())
                return true;
            __result = false;
            if (newBed.IsOwner(___pawn) || SharedBedUtility.HasPermanentBed(___pawn))
                return false;
            if (newBed.OwnersForReading.Count >= newBed.SleepingSlotsCount)
                return false;
            SharedBedUtility.ClaimTemporarily(___pawn, newBed);
            __result = true;
            return false;
        }
    }

    /// <summary>Release the shared bed as soon as the pawn stops lying in it.</summary>
    [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.Cleanup))]
    public static class JobDriver_Cleanup
    {
        // Runs after the job's finish actions, so vanilla bed thoughts are applied while the pawn still owns the bed.
        public static void Postfix(JobDriver __instance)
        {
            if (!(__instance is JobDriver_LayDown) || !(__instance.job?.targetA.Thing is Building_Bed bed))
                return;
            Pawn pawn = __instance.pawn;
            if (pawn == null || !bed.IsOwner(pawn) || !bed.IsShared())
                return;
            SharedBedUtility.Release(pawn, bed, addThought: true);
        }
    }
}
