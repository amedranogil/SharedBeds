using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SharedBeds
{
    public static class SharedBedUtility
    {
        // Writing the field directly skips the OwnedBed setter, which would wipe positive
        // "slept in bedroom" memories every time a pawn claims or releases a shared bed.
        private static readonly AccessTools.FieldRef<Pawn_Ownership, Building_Bed> ownedBedRef =
            AccessTools.FieldRefAccess<Pawn_Ownership, Building_Bed>("intOwnedBed");

        public static bool IsShared(this Building_Bed bed) => bed?.GetComp<CompSharedBed>()?.IsShared ?? false;

        public static bool IsGuestBed(Building_Bed bed) => bed.GetType().Name == "Building_GuestBed";

        /// <summary>Owns a bed that is not shared, i.e. a real personal bed.</summary>
        public static bool HasPermanentBed(Pawn pawn)
        {
            Building_Bed owned = pawn.ownership?.OwnedBed;
            return owned != null && !owned.IsShared();
        }

        public static bool CanUseSharedBeds(Pawn pawn, GuestStatus? guestStatus)
        {
            return guestStatus == null && pawn.IsFreeColonist && !pawn.IsSlave && pawn.ownership != null
                   && pawn.RaceProps.Humanlike && !pawn.DevelopmentalStage.Baby() && !HasPermanentBed(pawn);
        }

        public static void ClaimTemporarily(Pawn pawn, Building_Bed bed)
        {
            Building_Bed previous = pawn.ownership.OwnedBed;
            if (previous != null && previous != bed)
                Release(pawn, previous, addThought: false);
            bed.CompAssignableToPawn.ForceAddPawn(pawn);
            ownedBedRef(pawn.ownership) = bed;
        }

        public static void Release(Pawn pawn, Building_Bed bed, bool addThought)
        {
            bed.CompAssignableToPawn.ForceRemovePawn(pawn);
            if (pawn.ownership != null && pawn.ownership.OwnedBed == bed)
                ownedBedRef(pawn.ownership) = null;
            bed.GetComp<CompSharedBed>()?.Notify_Used(pawn);

            if (addThought && !pawn.Dead && pawn.needs?.mood != null && pawn.ownership?.OwnedBed == null
                && SharedBedsMod.Settings.noPersonalSpaceMood < 0)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(SharedBedsDefOf.SB_NoPersonalSpace);
            }
        }

        private static readonly List<Pawn> tmpOccupants = new List<Pawn>();

        /// <summary>
        /// Slot of an owner of a shared bed: the one it is lying in, otherwise the n-th free slot
        /// for the n-th owner not lying down yet. Stays the same while others come and go.
        /// </summary>
        public static int StableSleepingSlot(Building_Bed bed, Pawn pawn, int ownerIndex)
        {
            // GetCurOccupant only looks at positions, so this never calls back into IsOwner.
            int slots = bed.SleepingSlotsCount;
            tmpOccupants.Clear();
            for (int i = 0; i < slots; i++)
            {
                Pawn occupant = bed.GetCurOccupant(i);
                if (occupant == pawn)
                {
                    tmpOccupants.Clear();
                    return i;
                }
                tmpOccupants.Add(occupant);
            }

            List<Pawn> owners = bed.OwnersForReading;
            int rank = 0;
            for (int i = 0; i < ownerIndex && i < owners.Count; i++)
            {
                if (!tmpOccupants.Contains(owners[i]))
                    rank++;
            }
            int result = ownerIndex;
            for (int i = 0; i < slots; i++)
            {
                if (tmpOccupants[i] != null)
                    continue;
                if (rank == 0)
                {
                    result = i;
                    break;
                }
                rank--;
            }
            tmpOccupants.Clear();
            return result;
        }

        /// <summary>True while the pawn is lying in, or on its way to lie in, this bed.</summary>
        public static bool IsUsing(Pawn pawn, Building_Bed bed)
        {
            if (pawn.Dead || pawn.MapHeld != bed.Map)
                return false;
            if (pawn.CurrentBed() == bed)
                return true;
            Job job = pawn.CurJob;
            return job != null && pawn.jobs.curDriver is JobDriver_LayDown && job.targetA.Thing == bed;
        }

        public static Building_Bed FindSharedBedFor(Pawn sleeper, Pawn traveler, bool checkSocialProperness, bool ignoreOtherReservations)
        {
            Map map = sleeper.MapHeld;
            if (map == null)
                return null;

            bool hasLoverOnMap = false;
            List<DirectPawnRelation> lovers = LovePartnerRelationUtility.ExistingLovePartners(sleeper, allowDead: false);
            for (int i = 0; i < lovers.Count; i++)
            {
                if (lovers[i].otherPawn.MapHeld == map)
                    hasLoverOnMap = true;
            }

            Building_Bed best = null;
            float bestScore = float.MinValue;
            List<Building> buildings = map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                if (!(buildings[i] is Building_Bed bed) || !bed.IsShared())
                    continue;
                if (!RestUtility.CanUseBedEver(sleeper, bed.def))
                    continue;
                // Handles free slots, reachability, reservations, forbidden and "only share with lovers".
                if (!RestUtility.IsValidBedFor(bed, sleeper, traveler, checkSocialProperness, false, ignoreOtherReservations))
                    continue;

                float score = Score(sleeper, bed, hasLoverOnMap);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = bed;
                }
            }
            return best;
        }

        private static float Score(Pawn sleeper, Building_Bed bed, bool hasLoverOnMap)
        {
            float score = 0f;
            // Anyone already in the bed passed IsValidBedFor, so they are a lover.
            if (bed.OwnersForReading.Count > 0)
                score += 1000f;
            if (bed.GetComp<CompSharedBed>().WasRecentlyUsedBy(sleeper))
                score += 100f;
            // Keep double beds for couples, single beds for singles.
            if (bed.SleepingSlotsCount > 1)
                score += hasLoverOnMap ? 30f : -30f;

            Room room = bed.GetRoom();
            if (room == null || room.PsychologicallyOutdoors)
                score -= 200f;
            else
                score += room.GetStat(RoomStatDefOf.Impressiveness);

            score += bed.GetStatValue(StatDefOf.BedRestEffectiveness) * 50f;
            if (bed.Position.GetDangerFor(sleeper, bed.Map) > Danger.None)
                score -= 500f;
            score -= sleeper.PositionHeld.DistanceTo(bed.Position) * 0.1f;
            return score;
        }
    }
}
