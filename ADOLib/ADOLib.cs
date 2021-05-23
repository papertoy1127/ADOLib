using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using ADOLib.Settings;

namespace ADOLib
{
    public enum LogType
    {
        Normal,
        Warning,
        Error,
        Success
    }
    public static class ADOLib
    {
        public const int MajorVersion = 2;
        public const int MinorVersion = 0;
        public const int PatchVersion = 0;
        public static readonly string IdentifierVersion = "-pre1";
        public static readonly string Version = $"{MajorVersion}.{MinorVersion}.{PatchVersion}{IdentifierVersion}";
        
        public static GameObject Settings;
        public static Harmony harmony;
        public static UnityModManager.ModEntry ModEntry;
        public static string Path { get; private set; }
        public static String prefix = "ADOLib";
        internal static void Log(object log, LogType logType = LogType.Normal)
        {
            string color = "#ffffff";
            switch (logType)
            {
                case LogType.Normal:
                    color = "#ffffff";
                    break;
                case LogType.Warning:
                    color = "#F89B00";
                    break;
                case LogType.Error:
                    color = "#DD0000";
                    break;
                case LogType.Success:
                    color = "#DDDD00";
                    break;
            }
            UnityModManager.Logger.Log($"<color={color}>{log}</color>", $"[{prefix}] ");
        }

        public static void Setup(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;
            Path = modEntry.Path;
            Settings = new GameObject("ADOLib Settings");
            Settings.AddComponent<SettingsUI>();
            UnityEngine.Object.DontDestroyOnLoad(Settings);
            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAllByVersion(Assembly.GetExecutingAssembly());
            Log($"ADOLib Version {Version}");
        }
        
        [HarmonyPatch(typeof(scrController), "ValidInputWasTriggered")]
        private static class ValidInputWasTriggered
        {
            public static bool Prefix(ref bool __result)
            {
                if (SettingsUI.UI)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(PauseMenu), "Show")]
        private static class DontPause
        {
            public static bool Prefix()
            {
                if (SettingsUI.Escape)
                {
                    scrController.instance.TogglePauseGame();
                    SettingsUI.Escape = false;
                    return false;
                }
                return true;
            }
        }
    }
}