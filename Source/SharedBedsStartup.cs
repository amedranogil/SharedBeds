using RimWorld;
using Verse;

namespace SharedBeds
{
    [DefOf]
    public static class SharedBedsDefOf
    {
        public static ThoughtDef SB_NoPersonalSpace;

        static SharedBedsDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(SharedBedsDefOf));
    }

    /// <summary>Adds CompSharedBed to every humanlike bed def that supports owners.</summary>
    [StaticConstructorOnStartup]
    public static class SharedBedsStartup
    {
        static SharedBedsStartup()
        {
            int count = 0;
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (!def.IsBed || def.building == null || !def.building.bed_humanlike)
                    continue;
                if (!typeof(Building_Bed).IsAssignableFrom(def.thingClass) || !def.HasAssignableCompFrom(typeof(CompAssignableToPawn_Bed)))
                    continue;
                if (def.HasComp(typeof(CompSharedBed)))
                    continue;
                def.comps.Add(new CompProperties_SharedBed());
                count++;
            }
            ApplyMoodSetting();
            Log.Message($"[Shared Beds] Added shared bed option to {count} bed defs.");
        }

        public static void ApplyMoodSetting()
        {
            ThoughtDef thought = SharedBedsDefOf.SB_NoPersonalSpace;
            if (thought?.stages != null && thought.stages.Count > 0)
                thought.stages[0].baseMoodEffect = SharedBedsMod.Settings.noPersonalSpaceMood;
        }
    }
}
