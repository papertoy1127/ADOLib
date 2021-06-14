using System;
using System.Linq;
using System.Reflection;
using ADOLib.SafeTools;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using ADOLib.Settings;
//using ADOLib.SafeTools;
using ADOLib.Translation;

namespace ADOLib
{
    /// <summary>
    /// Logging type
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Normal log type.
        /// </summary>
        Normal,
        /// <summary>
        /// Warning log type.
        /// </summary>
        Warning,
        /// <summary>
        /// Error log type.
        /// </summary>
        Error,
        /// <summary>
        /// Success log type.
        /// </summary>
        Success,
        /// <summary>
        /// None log type.
        /// </summary>
        None
    }
    
    /// <summary>
    /// ADOLib main class
    /// </summary>
    public static class ADOLib
    {
        /// <summary>
        /// Major Version of ADOLib.
        /// </summary>
        public static readonly int MajorVersion = 2;
        
        /// <summary>
        /// Minor Version of ADOLib.
        /// </summary>
        public static readonly int MinorVersion = 1;
        
        /// <summary>
        /// Patch Version of ADOLib.
        /// </summary>
        public static readonly int PatchVersion = 1;

        /// <summary>
        /// Version identifier of ADOLib.
        /// </summary>
        public static readonly string VersionIdentifier = "";
        
        /// <summary>
        /// Version of ADOLib.
        /// </summary>
        public static readonly string Version = $"{MajorVersion}.{MinorVersion}.{PatchVersion}{VersionIdentifier}";
        
        /// <summary>
        /// Detected ADOFAI Release number.
        /// </summary>
        public static readonly int RELEASE_NUMBER_FIELD =
            (int) AccessTools.Field(typeof(GCNS), "releaseNumber").GetValue(null);
        
        internal static GameObject Settings;
        internal static Harmony harmony;
        internal static UnityModManager.ModEntry ModEntry;
        internal static Translator translator;
        internal static string Path { get; private set; }
        internal static string prefix = "ADOLib";
        internal static void Log(object log, LogType logType = LogType.Normal)
        {
            if (logType == LogType.None) {
                UnityModManager.Logger.Log($"{log}", $"[{prefix}] ");
                return;
            }
            string color = "#ffffff";
            string pre = "";
            switch (logType)
            {
                case LogType.Normal:
                    color = new Color(0.5f, 0.75f, 1).ToHex();
                    break;
                case LogType.Warning:
                    color = "#F89B00";
                    pre = "Warning: ";
                    break;
                case LogType.Error:
                    color = "#DD0000";
                    pre = "Error: ";
                    break;
                case LogType.Success:
                    color = "#DDDD00";
                    break;
            }
            UnityModManager.Logger.Log($"<color={color}>{pre}{log}</color>", $"[{prefix}] ");
        }
        
        internal static void Log(object log, Color color)
        {
            UnityModManager.Logger.Log($"<color={color.ToHex()}>{log}</color>", $"[{prefix}] ");
        }

        /// <summary>
        /// ADOLib Setup method.
        /// </summary>
        /// <param name="modEntry">ModEntry of the mod.</param>
        public static void Setup(UnityModManager.ModEntry modEntry) {
            GUIExtended.ArialFont = Font.CreateDynamicFontFromOSFont("Arial", 16);
            
            ModEntry = modEntry;
            Path = modEntry.Path;
            Settings = new GameObject("ADOLib Settings");
            Settings.AddComponent<SettingsUI>();
            UnityEngine.Object.DontDestroyOnLoad(Settings);
            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            translator = new Translator(modEntry.Path);
            
            Log($"ADOLib Version {Version}");
            Log($"ADOFAI Release r{RELEASE_NUMBER_FIELD}");
        }
    }

    [HarmonyPatch(typeof(UnityModManager), "_Start")]
    internal static class BeforeLoadPatch {
        public static void Prefix() => Category.RegisterCategories(AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()));
    }
}