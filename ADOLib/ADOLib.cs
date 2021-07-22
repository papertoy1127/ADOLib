using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ADOLib.Settings;
using ADOLib.Translation;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Object = UnityEngine.Object;

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
    public class ADOLib: ADOLibMod
    {
        internal const string _MajorVersion = "2";
        internal const string _MinorVersion = "2";
        internal const string _PatchVersion = "0";
        internal const string _VersionIdentifier = "";
        internal const string _Version = _MajorVersion + "." + _MinorVersion + "." + _PatchVersion + _VersionIdentifier;
        
        /// <summary>
        /// Major Version of ADOLib.
        /// </summary>
        public static readonly int MajorVersion = int.Parse(_MajorVersion);
        
        /// <summary>
        /// Minor Version of ADOLib.
        /// </summary>
        public static readonly int MinorVersion = int.Parse(_MinorVersion);
        
        /// <summary>
        /// Patch Version of ADOLib.
        /// </summary>
        public static readonly int PatchVersion = int.Parse(_PatchVersion);

        /// <summary>
        /// Version identifier of ADOLib.
        /// </summary>
        public static readonly  string VersionIdentifier = _VersionIdentifier;
        
        /// <summary>
        /// Version of ADOLib.
        /// </summary>
        public static readonly string Version = _Version;

        /// <summary>
        /// Detected ADOFAI Release number.
        /// </summary>
        public static readonly int RELEASE_NUMBER_FIELD =
            (int) AccessTools.Field(typeof(GCNS), "releaseNumber").GetValue(null);

        public override string Path => PathStatic;
        public static string PathStatic => Directory.GetCurrentDirectory() + "\\Mods\\ADOLib\\";

        internal static GameObject Settings;
        internal static Translator translator;
        internal static string prefix = "ADOLib";

        internal static Queue<ADOLibMod> ToInitalize = new Queue<ADOLibMod>();
        
        internal static void Log(object log, LogType logType = LogType.Normal)
        {
            if (logType == LogType.None) {
                MelonLogger.Msg($"[{prefix}] {log}");
                return;
            }

            ConsoleColor color = ConsoleColor.White;
            switch (logType)
            {
                case LogType.Normal:
                    color = ConsoleColor.Cyan;
                    break;
                case LogType.Warning:
                    color = ConsoleColor.Yellow;
                    MelonLogger.Warning($"{log}");
                    return;
                case LogType.Error:
                    color = ConsoleColor.Red;
                    MelonLogger.Error($"{log}");
                    return;
                case LogType.Success:
                    color = ConsoleColor.Blue;
                    break;
            }
            MelonLogger.Msg(color, $"{log}");
        }
        
        internal static void Log(object log, ConsoleColor color)
        {
            MelonLogger.Msg(color, $"{log}");
        }
        public static SettingsUI UI;

        /// <summary>
        /// ADOLib Setup method.
        /// </summary>
        public override void OnApplicationStart() {
            GUIExtended.ArialFont = Font.CreateDynamicFontFromOSFont("Arial", 16);

            Log($"ADOLib Version {Version}");
            Log($"ADOFAI Release r{RELEASE_NUMBER_FIELD}");
        }

        internal bool isInited = false;
        public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
            if (isInited) return;
            isInited = true;
            Settings = new GameObject("ADOLib Settings");
            Object.DontDestroyOnLoad(Settings);
            UI = Settings.AddComponent<SettingsUI>();
            while (ToInitalize.TryDequeue(out var mod)) {
                mod.Init();
            }
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            translator = new Translator(Path);
        }
    }
}