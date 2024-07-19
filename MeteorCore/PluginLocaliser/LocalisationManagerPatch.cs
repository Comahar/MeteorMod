using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace MeteorCore.Localiser.Patches;

[HarmonyPatch(typeof(LocalisationManager), nameof(LocalisationManager.TranslateDialogue))]
public class PluginLocalisationPatcher {
    // speed could be improved by using transpiler
    // use postfix with text.Contains("No translation found")
    public static void Postfix(ref string __result) {
        if(__result.Contains("No translation found")) {
            // clean result from "No Translation Found for 'word' in MISC"
            // to just "word" and "MISC"
            string text = __result.Replace("No translation found for '", "");
            text = text.Substring(0, text.IndexOf("' in"));

            string dictionaryName = __result.Substring(__result.IndexOf(" in ") + 4);

            var newText = PluginLocaliser.Translate(text, dictionaryName);
            __result = newText;
        }
    }

    /* Original code
    * if(Application.isPlaying && text.Contains("No translation found")) {
    *     Debug.LogError(text);
    *     text = LocalizationSettings.StringDatabase.GetLocalizedString(TableReference.op_Implicit(database), TableEntryReference.op_Implicit(locIndex.Trim()), LocalizationSettings.AvailableLocales.GetLocale(LocaleIdentifier.op_Implicit("en")), (FallbackBehavior)0, Array.Empty<object>());
    * }
    * return text;
    */
    // This just removes the Debug.LogError(text) line, only done for cleaner console output
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
        var codes = new List<CodeInstruction>(instructions);
        for(int i = 0; i < codes.Count; i++) {
            if(
                codes[i].opcode == OpCodes.Ldloc_0 &&
                codes[i + 1].opcode == System.Reflection.Emit.OpCodes.Call &&
                codes[i + 1].operand is MethodInfo method &&
                method.Name == "LogError"
            ) {
                codes[i].opcode = OpCodes.Nop;
                codes[i + 1].opcode = OpCodes.Nop;
                Plugin.Logger.LogInfo("LocalisationManager.TranslateDialogue Patch Success");
                return codes;
            }
        }
        Plugin.Logger.LogError("LocalisationManager.TranslateDialogue Patch Failed");
        return codes;
    }
}