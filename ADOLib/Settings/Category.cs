using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using ADOLib.Translation;
using UnityModManagerNet;

namespace ADOLib.Settings
{
    public abstract class Category : UnityModManager.ModSettings
    {
        public string TabName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public abstract UnityModManager.ModEntry ModEntry { get; }
        public abstract void OnGUI();
        public virtual void SaveAuto() => Save(ModEntry);

        /// <summary>
        /// Whether the tweak is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Whether the tweak's settings are expanded in the ADOLib menu.
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Gets the path to the file that holds this object's data.
        /// </summary>
        /// <param name="modEntry">The UMM mod entry for AdofaiTweaks.</param>
        /// <returns>
        /// The path to the file that holds this object's data.
        /// </returns>
        public override string GetPath(UnityModManager.ModEntry modEntry)
        {
            return Path.Combine(modEntry.Path, GetType().Name + ".xml");
        }

        /// <summary>
        /// Saves the settings data to the file at the path returned from
        /// <see cref="GetPath(UnityModManager.ModEntry)"/>.
        /// </summary>
        /// <param name="modEntry">The UMM mod entry for AdofaiTweaks.</param>
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            var filepath = GetPath(modEntry);
            try
            {
                using (var writer = new StreamWriter(filepath))
                {
                    var serializer = new XmlSerializer(GetType());
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception e)
            {
                modEntry.Logger.Error($"Can't save {filepath}.");
                modEntry.Logger.LogException(e);
            }
        }

        public static void RegisterCategories(IEnumerable<Type> types)
        {
            var categoryTypes = types.Where(t => t.GetCustomAttribute<CategoryAttribute>(true) != null)
                .OrderBy(t => t.GetCustomAttribute<CategoryAttribute>(true).Priority)
                .ThenBy(t => t.Name)
                .ToList();

            foreach (var category in categoryTypes)
            {
                var categoryAttr = category.GetCustomAttribute<CategoryAttribute>(true);
                var tabName = categoryAttr.TabName;

                var instance = (Category) Activator.CreateInstance(category);
                instance.Name = categoryAttr.Name;
                instance.Description = categoryAttr.Description;
                instance.TabName = categoryAttr.TabName;

                if (!SettingsUI.Tabs.Contains(tabName))
                {
                    SettingsUI.Categories[tabName] = instance;
                    SettingsUI.Settings[tabName] = instance.OnGUI;
                    SettingsUI.Saves[tabName] = instance.SaveAuto;
                    SettingsUI.Tabs.Add(tabName);
                }
                else
                {
                    SettingsUI.Settings[tabName] += instance.OnGUI;
                    SettingsUI.Saves[tabName] += instance.SaveAuto;
                }
            }
        }
    }
}