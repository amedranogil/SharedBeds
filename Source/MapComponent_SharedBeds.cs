using System.Collections.Generic;
using RimWorld;
using Verse;

namespace SharedBeds
{
    /// <summary>
    /// Safety net for claims not released by the LayDown job cleanup
    /// (save/load, caravans, despawned pawns, mod-added sleeping jobs...).
    /// </summary>
    public class MapComponent_SharedBeds : MapComponent
    {
        private const int SweepInterval = 250;
        private static readonly List<Pawn> tmpOwners = new List<Pawn>();

        public MapComponent_SharedBeds(Map map) : base(map) { }

        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % SweepInterval != 0)
                return;

            List<Building> buildings = map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                if (!(buildings[i] is Building_Bed bed) || bed.OwnersForReading.Count == 0 || !bed.IsShared())
                    continue;
                tmpOwners.Clear();
                tmpOwners.AddRange(bed.OwnersForReading);
                foreach (Pawn owner in tmpOwners)
                {
                    if (!SharedBedUtility.IsUsing(owner, bed))
                        SharedBedUtility.Release(owner, bed, addThought: false);
                }
            }
            tmpOwners.Clear();
        }
    }
}
