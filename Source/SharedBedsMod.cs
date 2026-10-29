using HarmonyLib;
using UnityEngine;
using Verse;

namespace SharedBeds
{
    public class SharedBedsSettings : ModSettings
    {
        public bool preferSharedOverUnowned = true;
        public int noPersonalSpaceMood = -2;
        public int colonistsPerSharedSlot = 3;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref preferSharedOverUnowned, "preferSharedOverUnowned", true);
            Scribe_Values.Look(ref noPersonalSpaceMood, "noPersonalSpaceMood", -2);
            Scribe_Values.Look(ref colonistsPerSharedSlot, "colonistsPerSharedSlot", 3);
        }
    }

    public class SharedBedsMod : Mod
    {
        public static SharedBedsSettings Settings;

        public SharedBedsMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SharedBedsSettings>();
            new Harmony("mec.sharedbeds").PatchAll();
        }

        public override string SettingsCategory() => "Shared Beds";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);
            list.CheckboxLabeled("SB_Setting_PreferShared".Translate(), ref Settings.preferSharedOverUnowned, "SB_Setting_PreferSharedDesc".Translate());
            list.Gap();
            list.Label("SB_Setting_Debuff".Translate(Settings.noPersonalSpaceMood), tooltip: "SB_Setting_DebuffDesc".Translate());
            Settings.noPersonalSpaceMood = Mathf.RoundToInt(list.Slider(Settings.noPersonalSpaceMood, -10f, 0f));
            list.Gap();
            list.Label("SB_Setting_AlertFactor".Translate(Settings.colonistsPerSharedSlot));
            Settings.colonistsPerSharedSlot = Mathf.RoundToInt(list.Slider(Settings.colonistsPerSharedSlot, 1f, 4f));
            list.End();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            SharedBedsStartup.ApplyMoodSetting();
        }
    }
}
