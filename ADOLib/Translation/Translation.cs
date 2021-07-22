using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace ADOLib.Translation
{
    /// <summary>
    /// ADOLib Translator class.
    /// </summary>
    public class Translator
    {
        /// <summary>
        /// Path the language files are located.
        /// </summary>
        public string Path { get; }

        private Dictionary<SystemLanguage, Dictionary<string, string>> StringsMap { get; }
        
        /// <summary>
        /// Instantiates new translator with path. 
        /// </summary>
        /// <param name="path">Path the language files are located.</param>
        public Translator(string path)
        {
            Path = path;
            StringsMap = new Dictionary<SystemLanguage, Dictionary<string, string>>();
            var lPath = Path + Setting.LanguageDirectory;
            if (!Directory.Exists(lPath))
                Directory.CreateDirectory(lPath);
            var langFiles = Directory.GetFiles(lPath);
            ADOLib.Log(langFiles.Length);
            foreach (var langFile in langFiles)
            {
                ADOLib.Log(langFile);
                SystemLanguage lang;
                var langNames = langFile.Split('\\');
                var langName = langNames[langNames.Length - 1].Replace(".lang", "");
                if (!Enum.TryParse(langName, out lang)) continue;
                
                var builder = new DeserializerBuilder().Build();
                Dictionary<string, string> langStrings;
                using (var stream = File.Open(langFile, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        try
                        {
                            langStrings =
                                builder.Deserialize(reader, typeof(Dictionary<string, string>)) as
                                    Dictionary<string, string>;
                        }
                        catch (YamlException)
                        {
                            ADOLib.Log($"Cannot load language {lang} from {path}", LogType.Error);
                            langStrings = new Dictionary<string, string>();
                        }
                    }
                }

                StringsMap[lang] = langStrings;
            }
            if (!StringsMap.ContainsKey(SystemLanguage.English))
            {
                StringsMap[SystemLanguage.English] = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Translates text with key.
        /// </summary>
        /// <param name="input">Key of the translated string</param>
        /// <returns>String translated into default language.</returns>
        public string Translate(string input)
        {
            return Translate(input, Setting.Language);
        }

        /// <summary>
        /// Translates text with key.
        /// </summary>
        /// <param name="input">Key of the translated string</param>
        /// <param name="language"><see cref="SystemLanguage">Language</see> to translate into.</param>
        /// <returns>String translated into the language.</returns>
        public string Translate(string input, SystemLanguage language)
        {
            if (!StringsMap.ContainsKey(language)) language = SystemLanguage.English;
            if (!StringsMap[language].ContainsKey(input))
            {
                ADOLib.Log(input);
                StringsMap[language][input] = $"{language}.{input}.NotTranslated";
                var serializer = new SerializerBuilder().Build();
                var path = Path + Setting.LanguageDirectory + $"{language}.lang";
                ADOLib.Log(path);
                File.WriteAllText(path, serializer.Serialize(StringsMap[language]));
            }
            return StringsMap[language][input].Replace("\\n", "\n");
        }
    }
}