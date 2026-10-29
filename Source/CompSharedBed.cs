using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace SharedBeds
{
    public class CompProperties_SharedBed : CompProperties
    {
        public CompProperties_SharedBed() => compClass = typeof(CompSharedBed);
    }

    public class CompSharedBed : ThingComp
    {
        public static readonly Color SheetColorShared = new Color(0.86f, 0.45f, 0.70f);

        private static Texture2D iconTex;
        private static Texture2D IconTex => iconTex ??= ContentFinder<Texture2D>.Get("UI/Commands/SB_SharedBed");

        private const int MaxRecentUsers = 8;

        private bool isShared;
        // Most recent first. Used so colonists return to the bed they slept in before.
        private List<Pawn> recentUsers = new List<Pawn>();

        public Building_Bed Bed => (Building_Bed)parent;

        /// <summary>Shared flag, only effective on non-medical colonist beds.</summary>
        public bool IsShared => isShared && CanBeShared;

        private bool CanBeShared
        {
            get
            {
                Building_Bed bed = Bed;
                return bed.ForColonists && !bed.Medical && !bed.ForHumanBabies && !SharedBedUtility.IsGuestBed(bed);
            }
        }

        public void SetShared(bool value)
        {
            if (value == isShared)
                return;
            isShared = value;
            Building_Bed bed = Bed;
            // Turning sharing on or off ends every current ownership (permanent or temporary).
            foreach (Pawn owner in bed.OwnersForReading.ToList())
                owner.ownership.UnclaimBed();
            if (value)
                bed.Medical = false;
            bed.Notify_ColorChanged();
            if (bed.Spawned)
                bed.Map.mapDrawer.MapMeshDirty(bed.Position, MapMeshFlagDefOf.Things);
        }

        /// <summary>Called after Medical / owner type changes. Vanilla already removed the owners.</summary>
        public void Notify_BedModeChanged()
        {
            if (isShared && !CanBeShared)
                isShared = false;
        }

        public void Notify_Used(Pawn pawn)
        {
            recentUsers.Remove(pawn);
            recentUsers.Insert(0, pawn);
            if (recentUsers.Count > MaxRecentUsers)
                recentUsers.RemoveRange(MaxRecentUsers, recentUsers.Count - MaxRecentUsers);
        }

        public bool WasRecentlyUsedBy(Pawn pawn) => recentUsers.Contains(pawn);

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.Faction != Faction.OfPlayer || !CanBeShared)
                yield break;
            yield return new Command_Toggle
            {
                defaultLabel = "SB_CommandSharedLabel".Translate(),
                defaultDesc = "SB_CommandSharedDesc".Translate(),
                icon = IconTex,
                isActive = () => isShared,
                toggleAction = () => SetShared(!isShared),
            };
        }

        // Vanilla already lists the current (temporary) owners.
        public override string CompInspectStringExtra() => IsShared ? "SB_SharedBed".Translate().Resolve() : null;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref isShared, "sharedBed");
            Scribe_Collections.Look(ref recentUsers, "sharedBedRecentUsers", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                recentUsers ??= new List<Pawn>();
                recentUsers.RemoveAll(p => p == null);
            }
        }
    }
}
