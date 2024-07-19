using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;
using BepInEx;
using MeteorCore.Localiser.Dictionaries;

namespace MeteorCore.Localiser {
    public static class PluginLocaliser {
        public static Dictionary<string, PluginDictionary> dictionaries = new Dictionary<string, PluginDictionary>();
        public static bool ignoreMissingTranslations = false;
        public static string language {
            // enum to string
            get { return System.Enum.GetName(typeof(LocalisationManager.Language), LocalisationManager.Instance.CurrentLanguage); }
        }

        public static string ConvertPluginToDictionaryName(BepInPlugin plugin) {
            return plugin.Name;
        }

        public static string Translate(string key, BepInPlugin plugin) {
            return Translate(key, ConvertPluginToDictionaryName(plugin));
        }

        /// <summary>
        /// First checks if the whole text is a key in the dictionary.
        /// If it is, it returns the translation.
        /// If not, it tries to translate as markup by temporarily removing the markup tags.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dictionaryName"></param>
        /// <param name="translateMarkup">Enables skipping of markup tags in the translation</param>
        /// <returns></returns>
        public static string Translate(string key, string dictionaryName, bool returnNullIfNotFound = false) {
            if(dictionaries.ContainsKey(dictionaryName)) {
                string translation = dictionaries[dictionaryName].Translate(key);
                if(!translation.IsNullOrWhiteSpace()) {
                    return translation;
                }
            }
            // Check additional dictionaries
            foreach(KeyValuePair<string, PluginDictionary> dict in dictionaries) {
                string translation = dict.Value.TranslateFromAdditionalDictionary(key, dictionaryName, language);
                if(!translation.IsNullOrWhiteSpace()) {
                    return translation;
                }
            }
            if(ignoreMissingTranslations) {
                return key;
            }

            Plugin.Logger.LogWarning($"Could not find translation for key {key} in dictionary {dictionaryName}");
            if(returnNullIfNotFound) {
                return null;
            }
            return GetTranslationNotFoundString(key, dictionaryName);
        }

        internal static string GetTranslationNotFoundString(string key, string dictionaryName) {
            return $"\"No '{key}' translation in {dictionaryName}\"";
        }


        // This assumes the plugin assembly has the same name as the plugin.name if projectName is null
        public static PluginDictionary RegisterPlugin(BepInPlugin plugin, string dictionaryPath = null, string projectName = null) {
            if(projectName == null) {
                projectName = plugin.Name;
            }
            // Check if plugin has already been registered
            string dictionaryName = ConvertPluginToDictionaryName(plugin);
            if(dictionaries.ContainsKey(dictionaryName)) {
                Plugin.Logger.LogWarning($"Plugin {projectName} already has a dictionary registered with name {dictionaryName}");
                return dictionaries[dictionaryName];
            }

            // Check if plugin has loaded assembly
            var loadedAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            System.Reflection.Assembly pluginAssembly = null;
            foreach(var assembly in loadedAssemblies) {
                if(assembly.GetName().Name == projectName) {
                    pluginAssembly = assembly;
                }
            }
            if(pluginAssembly == null) {
                Plugin.Logger.LogError($"Plugin {projectName} has no loaded assembly. Does it have the same name as the plugin?");
                return null;
            }

            // Get dictionary asset
            string dictionaryAssetPath = dictionaryPath ?? $"{projectName}.Assets.localisation.json";
            Stream stream = pluginAssembly.GetManifestResourceStream(dictionaryAssetPath);
            if(stream == null) {
                Plugin.Logger.LogError($"Could not find dictionary asset at path {dictionaryAssetPath} for plugin {plugin.Name}. Use embedded resource as the dictionary asset's Build Action property and place the dictionary inside the Assets folder.");
                return null;
            }

            using(StreamReader reader = new StreamReader(stream)) {
                string json = reader.ReadToEnd();
                if(string.IsNullOrEmpty(json)) {
                    Plugin.Logger.LogError($"Dictionary asset at path {dictionaryAssetPath} for plugin {plugin.Name} is empty");
                    return null;
                }

                // Parse dictionary
                PluginDictionary dictionary;
                try {
                    dictionary = JsonConvert.DeserializeObject<PluginDictionary>(json);
                    if(dictionary == null) {
                        Plugin.Logger.LogError($"Could not parse dictionary asset at path {dictionaryAssetPath} for plugin {plugin.Name}, it is null");
                        return null;
                    }
                } catch(System.Exception e) {
                    Plugin.Logger.LogError($"Could not parse dictionary asset at path {dictionaryAssetPath} for plugin {plugin.Name}");
                    Plugin.Logger.LogError(e);
                    throw e;
                }

                // Set other fields
                dictionary.name = dictionaryName;
                dictionary.plugin = plugin;

                dictionaries.Add(dictionaryName, dictionary);

                Plugin.Logger.LogInfo($"Plugin {projectName} registered with dictionary {dictionaryName}");
                return dictionary;
            }
        }
    }
}


namespace MeteorCore.Localiser.Dictionaries {
    [JsonObject(MemberSerialization.OptIn)]
    public class PluginDictionary {
        public string name { get; set; }
        public BepInPlugin plugin { get; set; }

        /// <summary>
        /// Dictionary<language, Dictionary<key, value>>
        /// </summary>
        [JsonProperty("dictionary")]
        public Dictionary<string, Dictionary<string, string>> dictionary { get; set; }

        /// <summary>
        /// This is used for custom addtions to other dictionaries to be used where dictionary injection is not possible.
        /// Dictionary<dictionaryName, Dictionary<language, Dictionary<key, value>>>
        /// </summary>
        [JsonProperty("additionalDictionaries")]
        public Dictionary<string, Dictionary<string, Dictionary<string, string>>> additionalDictionaries { get; set; }


        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) {
            if(this.dictionary == null) {
                Plugin.Logger.LogWarning($"Plugin {this.name} dictionary file does not include dictionary field");
                this.dictionary = new Dictionary<string, Dictionary<string, string>>();
            }
            if(this.additionalDictionaries == null) {
                Plugin.Logger.LogInfo($"Plugin {this.name} dictionary file does not include additionalDictionaries field");
                this.additionalDictionaries = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            }
        }

        public string Translate(string key) {
            return this.Translate(key, PluginLocaliser.language);
        }

        public string Translate(string key, string language) {
            if(this.dictionary.ContainsKey(language) && this.dictionary[language].ContainsKey(key)) {
                return this.dictionary[language][key];
            }
            if(PluginLocaliser.ignoreMissingTranslations) {
                return key;
            }
            return null;
        }

        public bool HasAdditionalDictionary(string dictionaryName) {
            return this.additionalDictionaries.ContainsKey(dictionaryName);
        }

        /// <summary>
        /// This is used for custom addtions to other dictionaries to be used where dictionary injection is not possible.
        /// Returns null if the key is not found in the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dictionaryName"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public string TranslateFromAdditionalDictionary(string key, string dictionaryName, string language) {
            if(this.additionalDictionaries.ContainsKey(dictionaryName)) {
                if(this.additionalDictionaries[dictionaryName].ContainsKey(language) && this.additionalDictionaries[dictionaryName][language].ContainsKey(key)) {
                    return this.additionalDictionaries[dictionaryName][language][key];
                }
            }
            return null;
        }

        public override string ToString() {
            string result = $"PluginDictionary: {this.name}\n";
            foreach(KeyValuePair<string, Dictionary<string, string>> language in this.dictionary) {
                result += $"{language.Key}\n";
                foreach(KeyValuePair<string, string> key in language.Value) {
                    result += $"{key.Key}: {key.Value}\n";
                }
            }
            foreach(KeyValuePair<string, Dictionary<string, Dictionary<string, string>>> additionalDictionaries in this.additionalDictionaries) {
                result += $"Additional Dictionaries: {additionalDictionaries.Key}\n";
                foreach(KeyValuePair<string, Dictionary<string, string>> language in additionalDictionaries.Value) {
                    result += $"{language.Key}\n";
                    foreach(KeyValuePair<string, string> key in language.Value) {
                        result += $"{key.Key}: {key.Value}\n";
                    }
                }
            }
            return result;
        }
    }
}
