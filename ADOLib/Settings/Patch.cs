using System.Linq;
using System.Reflection;
using ADOLib.SafeTools;
using ADOLib.Settings;
using HarmonyLib;
using UnityModManagerNet;

namespace ADOLib {
    public class Patch {
        [HarmonyPatch(typeof(UnityModManager.ModEntry), "Load")]
        private static class LoadModPatch {
            public static void Postfix(UnityModManager.ModEntry __instance) {
                var categories = __instance.Assembly.GetTypes().Where(t => t.GetCustomAttribute<CategoryAttribute>() != null);
                categories.Do(t => new Harmony(__instance.Info.Id + "_temp").PatchCategory(t));
            }
        }
        
        [SafePatch("ADOLib.ValidInputWasTriggeredPatch", "scrController", "ValidInputWasTriggered", -1, -1)]
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
        
        [SafePatch("ADOLib.ShowPauseMenu", "PauseMenu", "Show", -1, -1)]
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