using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using ADOLib.Settings;
//using ADOLib.SafeTools;
using ADOLib.Translation;

namespace ADOLib
{
    public enum LogType
    {
        Normal,
        Warning,
        Error,
        Success,
        None
    }
    public static class ADOLib
    {
        public static readonly int MajorVersion = 2;
        public static readonly int MinorVersion = 1;
        public static readonly int PatchVersion = 0;
        public static readonly string VersionIdentifier = "-pre1";
        public static readonly string Version = $"{MajorVersion}.{MinorVersion}.{PatchVersion}{VersionIdentifier}";
        
        public static readonly int RELEASE_NUMBER_FIELD =
            (int) AccessTools.Field(typeof(GCNS), "releaseNumber").GetValue(null);
        
        public static GameObject Settings;
        public static Harmony harmony;
        public static UnityModManager.ModEntry ModEntry;
        internal static Translator translator;
        public static string Path { get; private set; }
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

        public static void Setup(UnityModManager.ModEntry modEntry)
        {
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
}