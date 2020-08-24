using System;

namespace Microsoft.DotNet
{
    /// <summary>
    /// Specifies the level of configuration to use.
    /// </summary>
    public enum ConfigLevel
    {
        /// <summary>
        /// The global ~/.netconfig for the current user, from the 
        /// <see cref="Environment.SpecialFolder.UserProfile"/> location.
        /// </summary>
        /// <seealso cref="Config.GlobalLocation"/>
        Global,

        /// <summary>
        /// The system wide .netconfig, from the 
        /// <see cref="Environment.SpecialFolder.System"/> location.
        /// </summary>
        /// <seealso cref="Config.SystemLocation"/>
        System,
    }
}
