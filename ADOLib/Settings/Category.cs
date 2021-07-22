using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using ADOLib.SafeTools;
using UnityEngine;
using HarmonyLib;
using MelonLoader;

namespace ADOLib.Settings
{
    /// <summary>
    /// Category base class.
    /// </summary>
    public abstract class Category {
        internal static Dictionary<Type, Category> Categories = new Dictionary<Type, Category>();

        /// <summary>
        /// Get instance of the <see cref="Category"/>.
        /// </summary>
        /// <typeparam name="T">Category type to get the instance.</typeparam>
        /// <returns>Instance of the input Category. </returns>
        public static T GetCategory<T>() where T : Category, new() {
            return (T) GetCategory(typeof(T));
        }

        
        
        /// <summary>
        /// Get instance of the <see cref="Category"/>.
        /// </summary>
        /// <param name="type">Category type to get the instance.</param>
        /// <returns>Instance of the input Category. </returns>
        public static Category GetCategory(Type type) {
            if (Categories.ContainsKey(type)) {
                return Categories[type];
            }
            if (!(Activator.CreateInstance(type) is Category category)) {
                ADOLib.Log($"Type {type} is not a category!", LogType.Error);
                return null;
            }
            var mod = category.Mod;
            var path = category.XmlPath;
            if (File.Exists(path))
            {
                try {
                    using FileStream fileStream = File.OpenRead(path);
                    Categories[type] = (Category) new XmlSerializer(type).Deserialize(fileStream);
                    return Categories[type];
                }
                catch (Exception e)
                {
                    MelonLogger.Error("Can't read " + path + ".");
                    MelonLogger.Error(e);
                }

                Categories[type] = category;
                RegisterCategories(new [] {type});
                return Categories[type];
            }
            Categories[type] = category;
            RegisterCategories(new [] {type});
            return Categories[type];
        }
        
        /// <summary>
        /// Name of the Tab which this <see cref="Category"/> is in.
        /// </summary>
        public string TabName { get; set; }
        
        /// <summary>
        /// Name of this <see cref="Category"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ID of this <see cref="Category"/>.
        /// </summary>
        public string id => $"{GetType().Assembly.GetName().Name}+{GetType().FullName}";

        /// <summary>
        /// <see cref="Harmony"/> of this category.
        /// </summary>
        public HarmonyLib.Harmony harmony => Mod.HarmonyInstance;
        
        /// <summary>
        /// Type which inherits ADOLibMod and this <see cref="Category"/> is in.
        /// </summary>
        public abstract Type ModType { get; }

        internal ADOLibMod Mod => ADOLibMod.GetInstance(ModType);
        
        /// <summary>
        /// Invoked when <see cref="Category"/> setting is enabled and expanded.
        /// </summary>
        public abstract void OnGUI();

        /// <summary>
        /// Invoked when <see cref="Category"/> is loaded, regardless of whether it is enabled or not.
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// Whether the category is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Whether the category's settings are expanded in the ADOLib menu.
        /// </summary>
        public bool IsExpanded { get; set; } = true;
        
        /// <summary>
        /// Whether the category is registered in ADOLib.
        /// </summary>
        [NonSerialized]
        internal bool isRegistered = false;

        /// <summary>
        /// If this <see cref="Category"/> will be forced enable or disable.
        /// </summary>
        public ForceType ForceType { get; set; } = ForceType.DontForce;
        
        /// <summary>
        /// Whether this <see cref="Category"/> will be forced disable or not register when it is invalid.
        /// </summary>
        public InvalidMode InvalidMode { get; set; } = InvalidMode.Disable;

        /// <summary>
        /// The reason why this <see cref="Category"/> is <see cref="ForceType">Forced</see>.
        /// Ignored if <see cref="Settings.ForceType"/> is <see cref="Settings.ForceType.DontForce"/>.
        /// </summary>
        public string ForceReason = "";

        /// <summary>
        /// Invoked when this category is enabled.
        /// </summary>
        public virtual void OnEnable() {
            harmony.PatchCategory(GetType());
        }

        /// <summary>
        /// Invoked when this category is disabled.
        /// </summary>
        public virtual void OnDisable() {
            harmony.UnpatchCategory(GetType());
        }
        
        /// <summary>
        /// Invoked before this category is saved.
        /// </summary>
        public virtual void OnSave() { }
        
        /// <summary>
        /// The path to the file that holds this category's data.
        /// </summary>
        public string XmlPath {
            get {
                return Path.Combine(Mod.Path, ModType.Name + ".xml");
            }
        }

        /// <summary>
        /// <see cref="CategoryAttribute"/> of this category.
        /// </summary>
        public CategoryAttribute Metadata => GetType().GetCustomAttribute<CategoryAttribute>();

        /// <summary>
        /// Saves the settings data to the file at the <see cref="XmlPath"></see>.
        /// </summary>
        public void Save() {
            OnSave();
            var filepath = XmlPath;
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
                MelonLogger.Error($"Can't save {filepath}.");
                MelonLogger.Error(e);
            }
        }
        internal static void RegisterCategories(IEnumerable<Type> types)
        {
            ADOLib.Log("Registering Category");
            var categoryTypes = types.Where(t => t.GetCustomAttribute<CategoryAttribute>(true) != null).ToList();
            int success = 0;
            foreach (var category in categoryTypes)
            {
                var categoryAttr = category.GetCustomAttribute<CategoryAttribute>();
                var tabName = categoryAttr.TabName;

                var instance = GetCategory(category);
                if (instance.isRegistered) {
                    ADOLib.Log($"Category already registered, skipping category {category}", LogType.Warning);
                    continue;
                }
                instance.Name = categoryAttr.Name;
                instance.TabName = categoryAttr.TabName;
                instance.ForceType = categoryAttr.ForceType;
                instance.InvalidMode = categoryAttr.InvalidMode;
                instance.ForceReason = categoryAttr.ForceReason;
                if (!instance.Metadata.isValid) {
                    if (instance.InvalidMode == InvalidMode.UnRegister) {
                        ADOLib.Log($"Category version uncompatible, skipping category {category}", LogType.Warning);
                        continue;
                    }
                    instance.ForceType = ForceType.ForceDisable;
                    if (categoryAttr.MinVersion == -1)
                    {
                        instance.ForceReason = string.Format(ADOLib.translator
                                .Translate("Category.AdofaiVersionNotCompatible1"),
                            ADOLib.RELEASE_NUMBER_FIELD, categoryAttr.MaxVersion);
                    } else if (categoryAttr.MaxVersion == -1) {
                        instance.ForceReason = string.Format(ADOLib.translator
                                .Translate("Category.AdofaiVersionNotCompatible2"),
                            ADOLib.RELEASE_NUMBER_FIELD, categoryAttr.MinVersion);
                    }
                    else if (categoryAttr.MinVersion == categoryAttr.MaxVersion)
                    {
                        instance.ForceReason = string.Format(ADOLib.translator
                                .Translate("Category.AdofaiVersionNotCompatible3"),
                            ADOLib.RELEASE_NUMBER_FIELD, categoryAttr.MinVersion);
                    }
                    else {
                        instance.ForceReason = string.Format(ADOLib.translator
                                .Translate("Category.AdofaiVersionNotCompatible0"),
                            ADOLib.RELEASE_NUMBER_FIELD, categoryAttr.MinVersion, categoryAttr.MaxVersion);
                    }

                }
                if (instance.ForceType == ForceType.ForceDisable) instance.IsEnabled = false;
                else if (instance.ForceType == ForceType.ForceEnable) instance.IsEnabled = true;
                var text = GUIExtended.Text;
                text.alignment = TextAnchor.MiddleLeft;
                text.font = GUIExtended.ArialFont;
                text.fontSize = 20;

                Action OnGUI = () => {
                    GUILayout.BeginHorizontal();
                    instance.IsExpanded = GUILayout.Toggle(
                        instance.IsExpanded,
                        instance.IsEnabled ? instance.IsExpanded ? "▼" : "━" : "<color=#444444>━</color>",
                        new GUIStyle {
                            fixedWidth = 10,
                            normal = new GUIStyleState {textColor = Color.black},
                            fontSize = 20,
                            margin = new RectOffset(4, 16, 24, 0)
                        });
                    if (instance.ForceType == ForceType.DontForce) {
                        var wasEnabled = instance.IsEnabled;
                        instance.IsEnabled =
                            GUIExtended.Toggle(instance.IsEnabled, $"<size=50>{instance.Name}</size>");
                        if (instance.IsEnabled != wasEnabled)
                            if (instance.IsEnabled) instance.OnEnable();
                            else instance.OnDisable();
                    } else if (instance.ForceType == ForceType.ForceDisable) {
                        GUIExtended.Toggle(instance.IsEnabled, 
                            $"<size=50><color=#444444>{instance.Name}</color></size> <color=#555555>{instance.ForceReason}</color>",
                            GUIExtended.Text, "<color=#555555>☐</color>", "<color=#555555>☐</color>");
                    } else {
                        GUIExtended.Toggle(instance.IsEnabled, 
                            $"<size=50>{instance.Name}</size> <color=#555555>{instance.ForceReason}</color>",
                            GUIExtended.Text, "<color=#555555>☑</color>", "<color=#555555>☑</color>");
                    }

                    GUILayout.EndHorizontal();
                    if (instance.IsEnabled && instance.IsExpanded) {
                        GUIExtended.BeginIndent(15);
                        instance.OnGUI();
                        GUIExtended.EndIndent();
                    }
                    GUIExtended.HorizontalLine(0.2f);
                };
                if (!SettingsUI.Tabs.Contains(tabName))
                {
                    SettingsUI.Categories[tabName] = instance;
                    SettingsUI.Settings[tabName] = OnGUI;
                    SettingsUI.Saves[tabName] = instance.Save;
                    SettingsUI.Tabs.Insert(0, tabName);
                }
                else
                {
                    SettingsUI.Settings[tabName] += OnGUI;
                    SettingsUI.Saves[tabName] += instance.Save;
                }
                instance.Init();
                if (instance.IsEnabled) instance.OnEnable();
                else instance.OnDisable();
                ADOLib.Log($"Successfully registered category {category}", LogType.Success);
                success++;
            }
        }
    }
}