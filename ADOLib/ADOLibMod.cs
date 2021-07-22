using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;

namespace ADOLib {
    public abstract class ADOLibBaseMod: MelonMod {
        public sealed override void OnApplicationStart() {
            ADOLib.ToInitalize.Enqueue(this as ADOLibMod);
            ADOLibMod.instances[GetType()] = this as ADOLibMod;
            ADOLib.Log($"Start {GetType()}");
            (this as ADOLibMod).OnApplicationStart();
        }
    }

    public abstract class ADOLibMod : ADOLibBaseMod {
        internal static readonly Dictionary<Type, ADOLibMod> instances = new Dictionary<Type, ADOLibMod>();
        public new virtual void OnApplicationStart() { }
        
        /// <summary>
        /// Inits ADOLib-Related functions
        /// </summary>
        public virtual void Init() { }
        
        public static T GetInstance<T>() where T: ADOLibMod {
            ADOLibMod result;
            if (instances.TryGetValue(typeof(T), out result)) {
                return (T) result;
            }
            return null;
        }
        public static ADOLibMod GetInstance(Type type) {
            ADOLibMod result;
            if (instances.TryGetValue(type, out result)) {
                return result;
            }
            return null;
        }
        
        public virtual string Path => Directory.GetCurrentDirectory() + $"\\Mods\\{GetType().Name}\\";
    }
}