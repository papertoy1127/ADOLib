using System;

namespace ADOLib.Settings {
    /// <summary>
    /// Category metadata attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CategoryAttribute : System.Attribute {
        
        /// <summary>
        /// Name of the Tab which this <see cref="Category"/> is in.
        /// </summary>
        public string TabName { get; set; }
        
        /// <summary>
        /// Name of this <see cref="Category"/>.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Deprecated. No longer used.
        /// </summary>
        [Obsolete]
        public int Priority { get; set; } = 0;
        
        /// <summary>
        /// The patch class of this <see cref="Category"/>. Automatically patched when the <see cref="Category"/> is loaded.
        /// </summary>
        public Type PatchClass { get; set; }

        /// <summary>
        /// Minimum ADOFAI version required to enable this <see cref="Category"/>.
        /// </summary>
        public int MinVersion { get; set; } = -1;

        /// <summary>
        /// Maximum ADOFAI version required to enable this <see cref="Category"/>.
        /// </summary>
        public int MaxVersion { get; set; } = -1;

        /// <summary>
        /// ADOFAI branch required to enable this <see cref="Category"/>.
        /// </summary>
        public AdofaiBranch ADOFAIBranch { get; set; } = AdofaiBranch.All;

        
        /// <summary>
        /// If this <see cref="Category"/> will be forced enable or disable.
        /// </summary>
        public ForceType ForceType { get; set; } = ForceType.DontForce;
        
        /// <summary>
        /// Whether this <see cref="Category"/> will be forced disable or not register when it is invalid.
        /// </summary>
        public InvalidMode InvalidMode { get; set; } = InvalidMode.Disable;

        /// <summary>
        /// The reason why this <see cref="Category"/> is <see cref="ForceType">Forced</see>.
        /// Ignored if <see cref="Settings.ForceType"/> is <see cref="Settings.ForceType.DontForce"/>.
        /// </summary>
        public string ForceReason = "";

        /// <summary>
        /// Checks if this <see cref="Category"/> is valid with current ADOFAI version.
        /// </summary>
        public bool isValid =>
            (MinVersion <= ADOLib.RELEASE_NUMBER_FIELD || MinVersion == -1) &&
            (MaxVersion >= ADOLib.RELEASE_NUMBER_FIELD || MaxVersion == -1);
    }
}