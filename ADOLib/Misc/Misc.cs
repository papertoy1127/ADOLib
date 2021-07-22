using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace ADOLib.Misc
{
    public static class Misc
    {
        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                SingleOrDefault(assembly => assembly.GetName().Name == name);
        }
        public static string printEnumerator<T>(IEnumerable<T> enumerator) {
            StringBuilder result = new StringBuilder();
            enumerator.Do(obj => result.Append(obj + ", "));
            result.Remove(result.Length - 2, 2);
            return result.ToString();
        }
        
        public static T[] CastArray<T>(object[] input)
        {
            List<T> result = new List<T>();
            foreach (var item in input)
            {
                if (item is T) result.Add((T)item);
                else result.Add(null);
            }

            return result.ToArray();
        }
    
        public static T get<T>(this object o, string TargetName)
        {
            var info1 = o.GetType().GetField(TargetName, AccessTools.all);
            object result;
            if (info1 != null)
            {
                result = info1.GetValue(o);
                return (T) result;
            }

            var info2 = o.GetType().GetProperty(TargetName, AccessTools.all);
            result = info2.GetValue(o);
            return (T) result;
        }

        public static void setAll(this object o, string TargetName, object value)
        {
            var info1 = o.GetType().GetField(TargetName, AccessTools.all);
            object result;
            if (info1 != null)
            {
                result = info1.GetValue(o);
                info1.SetValue(o, value);
                return;
            }

            var info2 = o.GetType().GetProperty(TargetName, AccessTools.all);
            result = info2.GetValue(o);
            info2.SetValue(o, value);
        }
        
        public static void set<T>(this object o, string TargetName, T value)
        {
            var info1 = o.GetType().GetField(TargetName, AccessTools.all);
            object result;
            if (info1 != null)
            {
                result = info1.GetValue(o);
                info1.SetValue(o, value);
                return;
            }

            var info2 = o.GetType().GetProperty(TargetName, AccessTools.all);
            result = info2.GetValue(o);
            info2.SetValue(o, value);
        }

        public static T invoke<T>(this object o, string TargetName, object[] args)
        {
            var info = o.GetType().GetMethod(TargetName, AccessTools.all);
            var result = info.Invoke(o, args);
            return (T) result;
        }
    }
}