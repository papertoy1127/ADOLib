using System.Linq;
using System.Reflection;
using ADOLib.SafeTools;
//using ADOLib.SafeTools;
using ADOLib.Settings;
using HarmonyLib;
using UnityModManagerNet;

namespace ADOLib {
    internal static class Patch {
        [SafePatch("ADOLib.ValidInputWasTriggeredPatch", "scrController", "ValidInputWasTriggered")]
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
        
        [SafePatch("ADOLib.ShowPauseMenu", "PauseMenu", "Show")]
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