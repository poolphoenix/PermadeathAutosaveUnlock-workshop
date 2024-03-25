using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace PermadeathAutosaveUnlock
{
    public class PermadeathAutosaveUnlock : Mod
    {
        public PermadeathAutosaveUnlock(ModContentPack content) : base(content)
        {
            Log.Message("[PermadeathAutosaveUnlock] Initialized");
            var harmony = new Harmony("com.phoenix.PermadeathAutosaveUnlock");
            harmony.PatchAll();
        }
    }

    // Harmony patch for the AutosaveIntervalDays property, to skip the >1days check.
    [HarmonyPatch(typeof(Autosaver), "AutosaveIntervalDays", MethodType.Getter)]
    public static class AutosaveIntervalDays_PrefixPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref float __result)
        {
            __result = Prefs.AutosaveIntervalDays; // set __result to the autosave interval and skip the >1days check.
            return false;
        }
    }

    // This will patch the Options screen for the warning label.
    [HarmonyPatch(typeof(Dialog_Options), "DoGeneralOptions")]
    public static class DoGeneralOptions_Patch
    {
        // This static field will be used to keep track of whether the label should be skipped
        public static bool skipNextLabel = false;
        [HarmonyPrefix]
        public static void Prefix()
        {
            skipNextLabel = false; // Reset the flags at the start of the method
        }

        //Patch change the label that says maximum days allowed for commitment mode is 1 day.
        [HarmonyPatch(typeof(Listing_Standard), "Label", new Type[] { typeof(TaggedString), typeof(float), typeof(string) })]
        public static class Label_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref TaggedString label)
            {
                // If the label was changed already, skip the label check and return to the original method.
                if (skipNextLabel) return true;
                // Before a label is added, check if it's the one we want to skip
                if (label == "MaxPermadeathAutosaveIntervalInfo".Translate(1f))
                {
                    label = "AutosaveIntervalUnlocked".Translate().Colorize(Color.green); // Change the label text and paint it green.
                    skipNextLabel = true; // set the flag so it won't have to compare to other labels until next cycle, where it resets to false.
                }
                return true;
            }
        }
    }
}
