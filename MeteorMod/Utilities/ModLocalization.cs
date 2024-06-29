using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using MelonLoader;
using Newtonsoft.Json;
using HarmonyLib;

namespace MeteorMod.Utilities {

    [HarmonyPatch(typeof(LocalisationManager), nameof(LocalisationManager.TranslateDialogue))]
    public class ModLocalizationPatcher {
        /* Original code
        * if(Application.isPlaying && text.Contains("No translation found")) {
        *     Debug.LogError(text);
        *     text = LocalizationSettings.StringDatabase.GetLocalizedString(TableReference.op_Implicit(database), TableEntryReference.op_Implicit(locIndex.Trim()), LocalizationSettings.AvailableLocales.GetLocale(LocaleIdentifier.op_Implicit("en")), (FallbackBehavior)0, Array.Empty<object>());
        * }
        * return text;
        */
        // Remove the Debug.LogError(text) line
        // and use postfix with text.Contains("No translation found")
        public static void Postfix(ref string __result) {
            if(__result.Contains("No translation found")) {
                // clean result from "No Translation Found for 'word' in MISC"
                // to just "word"
                string t1 = __result.Replace("No translation found for '", "");
                t1 = t1.Substring(0, t1.IndexOf("' in"));
                var newText = ModLocalizationDictionary.Get(t1);
                if(!string.IsNullOrEmpty(newText)) {
                    __result = newText;
                } else if (ModLocalizationDictionary.Instance.IgnoreMissingTranslations) {
                    __result = t1;
                }
            }
        }

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
                    MelonLogger.Msg("LocalisationManager.TranslateDialogue Patch Success");
                    return codes;
                }
            }
            MelonLogger.Error("LocalisationManager.TranslateDialogue Patch Failed");
            return codes;
        }
    }

    public class ModLocalizationDictionary {

        private static readonly Lazy<ModLocalizationDictionary> _instance = new Lazy<ModLocalizationDictionary>(() => new ModLocalizationDictionary());

        public static ModLocalizationDictionary Instance => _instance.Value;

        // TODO get these from settings
        public string language = "en";
        public bool IgnoreMissingTranslations {
            get { return true; }
        }

        // Dictionary<language, Dictionary<internalText, uiText>>
        private Dictionary<string, Dictionary<string, string>> _localization = new Dictionary<string, Dictionary<string, string>>();

        // Parse and save the localization file
        public ModLocalizationDictionary() {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MeteorMod.localization.json");
            if(stream == null) {
                MelonLogger.Error("Could not find localization file");
                return;
            }

            using(StreamReader reader = new StreamReader(stream)) {
                string json = reader.ReadToEnd();
                if(string.IsNullOrEmpty(json)) {
                    MelonLogger.Error("Localization file is empty");
                    return;
                }
                var result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
                if(result == null) {
                    MelonLogger.Error("Could not parse localization file");
                    return;
                }
                _localization = result;
            }
            MelonLogger.Msg("Loaded localization file");
        }

        private Dictionary<string, string> GetLanguage(string language) {
            bool result = _localization.TryGetValue(language, out Dictionary<string, string> value);
            if(result) {
                return value;
            }
            MelonLogger.Error("Could not find language: " + language);
            return new Dictionary<string, string>();
        }

        public static string? Get(string key) {
            var languageDict = Instance.GetLanguage(Instance.language);

            bool result = languageDict.TryGetValue(key, out string value);
            //MelonLogger.Warning("Key: " + key + " Value: " + value);
            if(result) {
                return value;
            }
            return null;
        }
    }

}
