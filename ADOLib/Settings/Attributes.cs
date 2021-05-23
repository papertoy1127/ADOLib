using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ADOLib.Settings;
using HarmonyLib;

namespace ADOLib.Settings
{
    public enum HowPatch
    {
        Exclude,
        Only,
        Always,
    }

    public static class Setting
    {
        public static string printEnumerator<T>(IEnumerable<T> enumerator)
        {
            StringBuilder result = new StringBuilder();
            enumerator.Do(obj => result.Append(obj + ", "));
            result.Remove(result.Length - 2, 2);
            return result.ToString();
        }
        public static void PatchByVersion(this Harmony harmony, Type patch)
        {
            ADOLib.prefix = Assembly.GetCallingAssembly().GetName().Name;
            if (patch.GetCustomAttribute<HarmonyPatch>() == null)
            {
                ADOLib.Log($"Error: Couldn't patch {patch.FullName}: no HarmonyPatch found",
                    LogType.Error);
            }

            if (patch.GetCustomAttribute<AlternativeAttribute>() != null)
            {
                ADOLib.Log($"Warning: {patch.FullName} is alternative patch but patching it");
            }

            var attr = patch.GetCustomAttribute<PatchByVersionAttribute>();
            _PatchByVersion(harmony, patch, attr);
            ADOLib.prefix = "ADOLib";
        }

        private static void _PatchByVersion(Harmony harmony, Type patch, PatchByVersionAttribute attr)
        {
            if (attr == null)
            {
                attr = new PatchByVersionAttribute();
                attr.HowPatch = HowPatch.Always;
            }

            var version = attr.Versions;
            var alternative = attr.IfNot;
            switch (attr.HowPatch)
            {
                case HowPatch.Only:
                    if (version.Contains(GCNS.releaseNumber))
                    {
                        harmony.CreateClassProcessor(patch).Patch();
                        ADOLib.Log($"Successfully patched {patch.FullName}", LogType.Success);
                        return;
                    }

                    if (alternative == null)
                    {
                        ADOLib.Log(
                            $"Warning: Not patching {patch.FullName}; its patch version is {printEnumerator(version)}, but version is {GCNS.releaseNumber}",
                            LogType.Error);
                        return;
                    }
                    else
                    {
                        ADOLib.Log(
                            $"Warning: Not patching {patch.FullName}; its patch version is {printEnumerator(version)}, but version is {GCNS.releaseNumber}",
                            LogType.Warning);
                        ADOLib.Log($"Alternatively patching {alternative.FullName}", LogType.Warning);
                        _PatchByVersion(harmony, alternative, alternative.GetCustomAttribute<PatchByVersionAttribute>());
                    }
                    break;
                case HowPatch.Exclude:
                    if (!version.Contains(GCNS.releaseNumber))
                    {
                        harmony.CreateClassProcessor(patch).Patch();
                        ADOLib.Log($"Successfully patched {patch.FullName}", LogType.Success);
                        return;
                    }

                    if (alternative == null)
                    {
                        ADOLib.Log(
                            $"Warning: Not patching {patch.FullName}; its patch version is excluding {printEnumerator(version)}, but version is {GCNS.releaseNumber}",
                            LogType.Error);
                        return;
                    }
                    else
                    {
                        ADOLib.Log(
                            $"Warning: Not patching {patch.FullName}; its patch version is excluding {printEnumerator(version)}, but version is {GCNS.releaseNumber}",
                            LogType.Warning);
                        ADOLib.Log($"Alternatively patching {alternative.FullName}", LogType.Warning);
                        _PatchByVersion(harmony, alternative, alternative.GetCustomAttribute<PatchByVersionAttribute>());
                    }
                    break;
                case HowPatch.Always:
                    harmony.CreateClassProcessor(patch).Patch();
                    ADOLib.Log($"Successfully patched {patch.FullName}", LogType.Success);
                    return;
                default:
                    goto case HowPatch.Only;
            }

        }

        public static void PatchAllByVersion(this Harmony harmony, Assembly assembly)
        {
            ADOLib.prefix = Assembly.GetCallingAssembly().GetName().Name;
            var patchAttrs = assembly.GetTypes().Where(t => t.GetCustomAttribute<HarmonyPatch>(true) != null && t.GetCustomAttribute<AlternativeAttribute>() == null);
            patchAttrs.Do(type => _PatchByVersion(harmony, type, type.GetCustomAttribute<PatchByVersionAttribute>()));
            ADOLib.prefix = "ADOLib";
        }
    }

    
    [AttributeUsage(AttributeTargets.Class)]
    public class PatchByVersionAttribute : Attribute
    {
        public int[] Versions;
        public HowPatch HowPatch;
        public Type IfNot;
        
        public PatchByVersionAttribute(HowPatch howPatch = HowPatch.Only, Type ifNot = null, params int[] versions)
        {
            Versions = versions;
            HowPatch = howPatch;
            IfNot = ifNot;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AlternativeAttribute : Attribute
    {
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class OnGUIAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CategoryAttribute : Attribute
    {
        public string TabName { get; }
        public string Name { get; }
        public string Description { get; }
        public int Priority { get; }
        public CategoryAttribute(string tabName, string name, string description = "", int priority = 0)
        {
            Name = name;
            TabName = tabName;
            Description = description;
            Priority = priority;
        }
    }
}
