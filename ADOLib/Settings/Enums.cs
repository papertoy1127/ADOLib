using System;

namespace ADOLib.Settings {
    [Flags]
    public enum AdofaiBranch {
        None = 0x00000,
        Oct312020 = 0x00001,
        Public = 0x00010,
        Beta = 0x00100,
        Alpha = 0x01000,
        Stardust = 0x10000,
        
        PublicBeta = Public | Beta,
        ClosedBeta = Alpha | Stardust,
        DevVersion = Beta | Alpha | Stardust,
        All = Oct312020 | Public | Beta | Alpha | Stardust
    }

    /// <summary>
    /// Whether to force/disable categories.
    /// </summary>
    public enum ForceType {
        /// <summary>
        /// Do not force enable or disable.
        /// </summary>
        DontForce,

        /// <summary>
        /// Force enable.
        /// </summary>
        ForceEnable,

        /// <summary>
        /// Force disable.
        /// </summary>
        ForceDisable
    }

    /// <summary>
    /// How invalid categories will be registered.
    /// </summary>
    public enum InvalidMode {
        /// <summary>
        /// Disable if invalid.
        /// </summary>
        Disable,

        /// <summary>
        /// Unregister if invalid.
        /// </summary>
        UnRegister
    }
}