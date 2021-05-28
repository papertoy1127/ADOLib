using HarmonyLib;
using System;
using System.Reflection;
using ADOLib.Misc;

namespace ADOLib.SafeTools {
    /// <summary>
    /// Replaces <see cref="HarmonyPatch"/> and prevents mod crashing from having no class specified in the game's code.
    /// </summary>
    public class SafePatchAttribute : Attribute
    {
        public SafePatchAttribute(string patchId, string className, string methodName) {
            PatchId = patchId;
            ClassName = className;
            MethodName = methodName;
            MinVersion = -1;
            MaxVersion = -1;
            Assembly = Assembly.GetAssembly(typeof(ADOBase));
        }
        public SafePatchAttribute(string patchId, string className, string methodName, int minVersion, int maxVersion) {
            PatchId = patchId;
            ClassName = className;
            MethodName = methodName;
            MinVersion = minVersion;
            MaxVersion = maxVersion;
            Assembly = Assembly.GetAssembly(typeof(ADOBase));
        }

        public SafePatchAttribute(string patchId, string className, string methodName, int minVersion, int maxVersion,
            string assemblyName) {
            PatchId = patchId;
            ClassName = className;
            MethodName = methodName;
            MinVersion = minVersion;
            MaxVersion = maxVersion;
            try {
                Assembly = Misc.Misc.GetAssemblyByName(assemblyName);
            }
            catch (InvalidOperationException) {
                ADOLib.Log($"Error loading patch {patchId}: No assembly named {assemblyName}", LogType.Error);
            }
        }
        
        public SafePatchAttribute(string patchId, string className, string methodName, int minVersion, int maxVersion,
            Type assemblyType) {
            PatchId = patchId;
            ClassName = className;
            MethodName = methodName;
            MinVersion = minVersion;
            MaxVersion = maxVersion;
            Assembly = Assembly.GetAssembly(assemblyType);
        }

        /// <summary>
        /// Id of patch, it should <i>not</i> be identical to other patches' id.
        /// </summary>
        public string PatchId { get; set; }

        /// <summary>
        /// Name of the class to find specific method from.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Name of the method in the class to patch.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Minimum ADOFAI's version of this patch working.
        /// </summary>
        public int MinVersion { get; set; }

        /// <summary>
        /// Maximum ADOFAI's version of this patch working.
        /// </summary>
        public int MaxVersion { get; set; }
        
        /// <summary>
        /// Assembly to find target method from.
        /// </summary>
        public Assembly Assembly { get; set; }
        
        /// <summary>
        /// Whether the patch is patched (enabled).
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}