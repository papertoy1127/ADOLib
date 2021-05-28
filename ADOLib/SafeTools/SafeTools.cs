using System;
using System.Linq;
using System.Reflection;

namespace ADOLib.SafeTools {
    public static class SafeTools {
        public static void SafelyRun(int minVersion, int maxVersion, Action run, Action ifNot) {
            if ((minVersion <= ADOLib.RELEASE_NUMBER_FIELD || minVersion == -1) &&
                (maxVersion >= ADOLib.RELEASE_NUMBER_FIELD || maxVersion == -1))
                run();
            else
                ifNot();
        }
        
        public static void SafelyRun(string className, Action run, Action ifNot) {
            if (Assembly.GetAssembly(typeof(ADOBase)).GetType(className) != null)
                run();
            else
                ifNot();
        }
        
        public static void SafelyRun(string className, string memberName, Action run, Action ifNot) {
            var targetClass = Assembly.GetAssembly(typeof(ADOBase)).GetType(className);
            var member = targetClass.GetMembers().Where(info => info.Name == memberName);
            if (member.Count() != 0)
                run();
            else
                ifNot();
        }
    }
}