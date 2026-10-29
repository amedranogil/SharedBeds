using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SharedBeds.Patches
{
    /// <summary>Each shared sleeping slot covers several colonists for the "need colonist beds" alert.</summary>
    [HarmonyPatch(typeof(Alert_NeedColonistBeds), nameof(Alert_NeedColonistBeds.AvailableColonistBeds))]
    public static class Alert_NeedColonistBeds_AvailableColonistBeds
    {
        public static void Postfix(Map map, ref int singleBeds, ref int doubleBeds)
        {
            int extraPerBed = SharedBedsMod.Settings.colonistsPerSharedSlot - 1;
            if (extraPerBed <= 0)
                return;

            int sharedSingles = 0, sharedDoubles = 0;
            List<Building> buildings = map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i] is Building_Bed bed && !bed.ForHumanBabies && bed.IsShared())
                {
                    if (bed.SleepingSlotsCount == 1)
                        sharedSingles++;
                    else
                        sharedDoubles++;
                }
            }
            if (sharedSingles + sharedDoubles == 0)
                return;

            singleBeds += sharedSingles * extraPerBed;
            doubleBeds += sharedDoubles * extraPerBed;
            // Vanilla uses up double beds before going negative on single beds.
            if (singleBeds < 0 && doubleBeds > 0)
            {
                int moved = Math.Min(-singleBeds, doubleBeds);
                singleBeds += moved;
                doubleBeds -= moved;
            }
        }
    }
}
