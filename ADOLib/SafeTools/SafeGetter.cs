using System;
using System.Collections.Generic;
using System.Reflection;

namespace ADOLib.SafeTools {
    public class SafeGetter<T> {
        public readonly MemberInfo MemberInfo;
        public SafeGetter(params string[] infos) {
            foreach (var info in infos) {
                var f = typeof(T).GetField(info);
                if (f != null && f.FieldType == typeof(T)) {
                    MemberInfo = f;
                    break;
                }
                var p = typeof(T).GetProperty(info);
                if (p == null || p.PropertyType != typeof(T)) continue;
                MemberInfo = p;
                break;
            }
        }
    }

    public static class SafeGetterHelper {
        public static T SafeGetter<T>(this T instance, SafeGetter<T> getter) {
            if (getter.MemberInfo == null) return default;
            if (getter.MemberInfo is FieldInfo f) return (T) f.GetValue(instance);
            return (T) (getter.MemberInfo as PropertyInfo)?.GetValue(instance);
        }
    }
}