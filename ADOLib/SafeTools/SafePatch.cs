using System;
using System.Linq;
using System.Reflection;
using ADOLib.Settings;
using HarmonyLib;

namespace ADOLib.SafeTools {

    /// <summary>
    /// Tweak's patch system specifically designed to work at all versions of the game to avoid mod from crashing.
    /// </summary>
    public static class SafePatchHelper {

        /// <summary>
        /// Checks whether the patch is valid for current game's version.
        /// </summary>
        /// <param name="patchType">Type of the patching method.</param>
        /// <param name="showDebuggingMessage">Whether to show debugging message in logs.</param>
        /// <returns>Patch's current availability in <see cref="bool"/>.</returns>
        public static bool IsValidPatch(Type patchType, bool showDebuggingMessage = false) {
            showDebuggingMessage = true;
            var patchAttr = patchType.GetCustomAttribute<SafePatchAttribute>();
            if (patchAttr == null) {
                if (showDebuggingMessage) {
                    ADOLib.Log($"Patch Type {patchType.FullName} is invalid! - no SafePatch attribute found!\n");
                }
                return false;
            }
            var classType = patchAttr.Assembly.GetType(patchAttr.ClassName);
            
            if ((patchAttr.MinVersion <= ADOLib.RELEASE_NUMBER_FIELD || patchAttr.MinVersion == -1) &&
                (patchAttr.MaxVersion >= ADOLib.RELEASE_NUMBER_FIELD || patchAttr.MaxVersion == -1) &&
                classType != null) {
                return true;
            }

            if (showDebuggingMessage) {
                ADOLib.Log($"Patch {patchAttr.PatchId} is invalid! - Specific criteria check:\n" +
                                        $"Metadata.MinVersion <= ADOLib.RELEASE_NUMBER_FIELD ({patchAttr.MinVersion} <= {ADOLib.RELEASE_NUMBER_FIELD}) is {patchAttr.MinVersion <= ADOLib.RELEASE_NUMBER_FIELD}\n" +
                                        $"Metadata.MinVersion <= ADOLib.RELEASE_NUMBER_FIELD ({patchAttr.MaxVersion} >= {ADOLib.RELEASE_NUMBER_FIELD}) is {patchAttr.MaxVersion >= ADOLib.RELEASE_NUMBER_FIELD}\n" +
                                        $"ClassType is {classType}\n" +
                                        $"PatchType is {patchType}\n", LogType.Warning);
            }
            return false;
        }

        /// <summary>
        /// Patches patch.
        /// <param name="harmony">Harmony class to apply patch.</param>
        /// <param name="patchType">Harmony class to apply patch.</param>
        /// </summary>
        public static void SafePatch(this Harmony harmony, Type patchType) {
            var metadata = patchType.GetCustomAttribute<SafePatchAttribute>();
            if (metadata == null) {
                ADOLib.Log($"Type {patchType} doesn't have SafePatch attribute.", LogType.Error);
                return;
            }
            ADOLib.Log($"Patching {metadata.PatchId}");
            
            if (metadata.IsEnabled) {
                ADOLib.Log($"{metadata.PatchId} is already patched!", LogType.Warning);
                return;
            }

            if (!IsValidPatch(patchType)) {
                ADOLib.Log($"Type {patchType} is not valid for this ADOFAI version", LogType.Warning);
                return;
            }

            Type declaringType = metadata.Assembly.GetType(metadata.ClassName);
            if (declaringType == null) {
                ADOLib.Log($"Type {metadata.ClassName} not found", LogType.Warning);
                return;
            }

            try {
                harmony.CreateClassProcessor(patchType).Patch();
            }
            catch (Exception) {
                ADOLib.Log($"Wrong patch method {metadata.MethodName}", LogType.Warning);
                return;
            }

            metadata.IsEnabled = true;
            var metadata2 = patchType.GetCustomAttribute<SafePatchAttribute>();
            ADOLib.Log($"Successfully patched {metadata.PatchId}", LogType.Success);
        }

        /// <summary>
        /// Unpatches patch.
        /// </summary>
        public static void SafeUnpatch(this Harmony harmony, Type patchType) {
            var metadata = patchType.GetCustomAttribute<SafePatchAttribute>();
            if (metadata == null) {
                ADOLib.Log($"Type {patchType} doesn't have SafePatch attribute.");
                return;
            }
            ADOLib.Log($"Unpatching {metadata.PatchId}");

            if (!metadata.IsEnabled) {
                ADOLib.Log($"{metadata.PatchId} is not patched!", LogType.Warning);
                return;
            }

            var classType = metadata.Assembly.GetType(metadata.ClassName);
            if (classType == null) {
                ADOLib.Log($"Type {metadata.ClassName} not found in assembly {metadata.Assembly}.");
                return;
            }

            var original = metadata.info.method;
            foreach (var patch in patchType.GetMethods()) {
                harmony.Unpatch(original, patch);
            }

            metadata.IsEnabled = false;
            ADOLib.Log($"Successfully unpatched {metadata.PatchId}", LogType.Success);
        }

        public static void PatchCategory<T>(this Harmony harmony) where T : Category {
            PatchCategory(harmony, typeof(T));
        }
        
        public static void UnpatchCategory<T>(this Harmony harmony) where T : Category {
            UnpatchCategory(harmony, typeof(T));
        }
        
        public static void PatchCategory(this Harmony harmony, Type type) {
            ADOLib.Log($"Patching category {type}");
            var patchAttr = type.GetCustomAttribute<CategoryAttribute>();
            if (!patchAttr.isValid) {
                ADOLib.Log($"{type} is not valid for this ADOFAI version", LogType.Warning);
                return;
            }
            var patchClass = patchAttr.PatchClass;
            if (patchClass == null) {
                ADOLib.Log($"No patch class found in category {type}", LogType.Warning);
                return;
            }
            var patches = patchClass.GetNestedTypes(AccessTools.all).Where(t => t.GetCustomAttribute<SafePatchAttribute>() != null);
            foreach (var p in patches) { harmony.SafePatch(p); }
                ADOLib.Log($"Successfully patched category {type}", LogType.Success);
        }
        
        public static void UnpatchCategory(this Harmony harmony, Type type) {
            ADOLib.Log($"Unpatching category {type}");
            var patchAttr = type.GetCustomAttribute<CategoryAttribute>();
            if (!patchAttr.isValid) {
                ADOLib.Log($"Not unpatching {type}; ADOFAI version is not compatible", LogType.Warning);
                return;
            }
            var patchClass = patchAttr.PatchClass;
            if (patchClass == null) {
                ADOLib.Log($"No patch class found in category {type}", LogType.Warning);
                return;
            }
            ADOLib.Log("Unpatching SafePatch...");
            var patches = patchClass.GetNestedTypes(AccessTools.all).Where(t => t.GetCustomAttribute<SafePatchAttribute>() != null);
            patches.Do(harmony.SafeUnpatch);
            ADOLib.Log($"Successfully patched category {type}", LogType.Success);
        }
    }
}