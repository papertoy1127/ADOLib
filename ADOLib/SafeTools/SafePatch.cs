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
        internal static bool IsValidPatch(Type patchType, bool showDebuggingMessage = false) {
            var patchAttr = patchType.GetCustomAttribute<SafePatchAttribute>();
            if (patchAttr == null) {
                if (showDebuggingMessage) {
                    ADOLib.Log($"Patch Type {patchType.FullName} is invalid! - no SafePatch attribute found!\n");
                }
                return false;
            }
            var classType = patchAttr.Assembly.GetType(patchAttr.ClassName);
            
            var patchTargetMethods = classType?.GetMethods().Where(m => m.Name.Equals(patchAttr.MethodName)).ToList();

            if ((patchAttr.MinVersion <= ADOLib.RELEASE_NUMBER_FIELD || patchAttr.MinVersion == -1) &&
                (patchAttr.MaxVersion >= ADOLib.RELEASE_NUMBER_FIELD || patchAttr.MaxVersion == -1) &&
                classType != null &&
                patchTargetMethods.Count != 0) {
                return true;
            }

            if (showDebuggingMessage) {
                ADOLib.Log($"Patch {patchAttr.PatchId} is invalid! - Specific criteria check:\n" +
                                        $"Metadata.MinVersion <= ADOLib.RELEASE_NUMBER_FIELD ({patchAttr.MinVersion} <= {ADOLib.RELEASE_NUMBER_FIELD}) is {patchAttr.MinVersion <= ADOLib.RELEASE_NUMBER_FIELD}\n" +
                                        $"Metadata.MinVersion <= ADOLib.RELEASE_NUMBER_FIELD ({patchAttr.MaxVersion} >= {ADOLib.RELEASE_NUMBER_FIELD}) is {patchAttr.MaxVersion >= ADOLib.RELEASE_NUMBER_FIELD}\n" +
                                        $"ClassType is {classType}\n" +
                                        $"PatchType is {patchType}\n" +
                                        $"PatchTargetMethods count is {patchTargetMethods?.Count ?? 0}", LogType.Warning);
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
                ADOLib.Log($"Error: Type {patchType} doesn't have SafePatch attribute.");
                return;
            }
            
            if (!metadata.IsEnabled) {
                var classType = metadata.Assembly.GetType(metadata.ClassName);
                if (classType == null) {
                    ADOLib.Log($"Error: Type {metadata.ClassName} not found in assembly {metadata.Assembly}.");
                    return;
                }
                var patchTargetMethods = classType.GetMethods().Where(m => m.Name.Equals(metadata.MethodName));
                
                foreach (MethodInfo method in patchTargetMethods) {
                    MethodInfo prefixMethodInfo = patchType.GetMethod("Prefix", AccessTools.all),
                        postfixMethodInfo = patchType.GetMethod("Postfix", AccessTools.all);

                    HarmonyMethod prefixMethod = null, 
                                  postfixMethod = null;

                    if (prefixMethodInfo != null) {
                        prefixMethod = new HarmonyMethod(prefixMethodInfo);
                    }

                    if (postfixMethodInfo != null) {
                        postfixMethod = new HarmonyMethod(postfixMethodInfo);
                    }
                    
                    ADOLib.Log($"{method}\n{prefixMethod}\n{postfixMethod}");

                    harmony.Patch(
                        method,
                        prefixMethod,
                        postfixMethod);
                    ADOLib.Log($"Successfully patched {metadata.PatchId}", LogType.Success);
                }

                metadata.IsEnabled = true;
            }
        }

        /// <summary>
        /// Unpatches patch.
        /// </summary>
        public static void SafeUnpatch(this Harmony harmony, Type patchType) {
            var metadata = patchType.GetCustomAttribute<SafePatchAttribute>();
            if (metadata == null) {
                ADOLib.Log($"Error: Type {patchType} doesn't have SafePatch attribute.");
                return;
            }
            
            if (!metadata.IsEnabled) {
                var classType = metadata.Assembly.GetType(metadata.ClassName);
                if (classType == null) {
                    ADOLib.Log($"Error: Type {metadata.ClassName} not found in assembly {metadata.Assembly}.");
                    return;
                }
                var patchTargetMethods = classType.GetMethods().Where(m => m.Name.Equals(metadata.MethodName));
                
                foreach (MethodInfo original in patchTargetMethods) {
                    foreach (var patch in patchType.GetMethods()) {
                        harmony.Unpatch(original, patch);
                        ADOLib.Log($"Successfully unpatched {metadata.PatchId}", LogType.Success);
                    }
                }

                metadata.IsEnabled = false;
            }
        }

        public static void SafePatchAll(this Harmony harmony, Assembly assembly) {
            var patches = assembly.GetTypes().Where(t => t.GetCustomAttribute<SafePatchAttribute>() != null);
            patches.Do(harmony.SafePatch);
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
                ADOLib.Log($"Not patching {type}; ADOFAI version is not compatible", LogType.Warning);
                return;
            }
            var patchClass = patchAttr.PatchClass;
            if (patchClass == null) {
                ADOLib.Log($"No patch class found in category {type}", LogType.Warning);
                return;
            }
            ADOLib.Log("Patching SafePatch...");
            var patches = patchClass.GetNestedTypes(AccessTools.all).Where(t => t.GetCustomAttribute<SafePatchAttribute>() != null);
            patches.Do(harmony.SafePatch);
            ADOLib.Log("Success!", LogType.Success);
            
            ADOLib.Log($"Patching HarmonyPatch...");
            var patches2 = patchClass.GetNestedTypes(AccessTools.all).Where(t => t.GetCustomAttribute<HarmonyPatch>() != null);
            patches2.Do(t => {
                harmony.CreateClassProcessor(t).Patch();
                ADOLib.Log($"Successfully patched HarmonyPatch {t}", LogType.Success);
            });
            ADOLib.Log("Success!", LogType.Success);
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
            ADOLib.Log("Success!", LogType.Success);
            
            ADOLib.Log("Unpatching HarmonyPatch...");
            var patches2 = patchClass.GetNestedTypes(AccessTools.all).Where(t => t.GetCustomAttribute<HarmonyPatch>() != null);
            patches2.Do(t => {
                var metadata = t.GetCustomAttribute<HarmonyPatch>();
                foreach (var method in t.GetMethods()) {
                    harmony.Unpatch(metadata.info.method, method);
                }
                ADOLib.Log($"Successfully unpatched HarmonyPatch {t}", LogType.Success);
            });
            ADOLib.Log("Success!", LogType.Success);
            ADOLib.Log($"Successfully patched category {type}", LogType.Success);
        }
    }
}