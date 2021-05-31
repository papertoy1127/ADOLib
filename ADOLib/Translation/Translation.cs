using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ADOLib.Translation
{
    public class Translator
    {
        public string Path { get; }
        public Dictionary<SystemLanguage, Dictionary<string, string>> StringsMap { get; }
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
                if (Enum.TryParse(langName, out lang))
                {
                    var builder = new DeserializerBuilder().Build();
                    Dictionary<string, string> langStrings;
                    using (var stream = File.Open(langFile, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            langStrings = builder.Deserialize(reader, typeof(Dictionary<string, string>)) as Dictionary<string, string>;
                        }
                    }

                    StringsMap[lang] = langStrings;
                }
            }
            if (!StringsMap.ContainsKey(SystemLanguage.English))
            {
                StringsMap[SystemLanguage.English] = new Dictionary<string, string>();
            }
        }

        public string Translate(string input)
        {
            return Translate(input, Setting.Language);
        }
        
        public string Translate(string input, SystemLanguage language)
        {
            if (!StringsMap.ContainsKey(language)) language = SystemLanguage.English;
            if (!StringsMap[language].ContainsKey(input))
            {
                ADOLib.Log(input);
                StringsMap[language][input] = $"{language}.{input}.NotTranslated";
                StringBuilder langStringBuilder = new StringBuilder();
                foreach (var langString in StringsMap[language])
                {
                    langStringBuilder.AppendLine($"{langString.Key}: {langString.Value}");
                }
                var path = Path + Setting.LanguageDirectory + $"{language}.lang";
                File.WriteAllText(path, langStringBuilder.ToString());
            }
            return StringsMap[language][input].Replace("\\n", "\n");
        }
    }
}