using System;

namespace DotNetConfig
{
    /// <summary>
    /// Specifies the level of configuration to use.
    /// </summary>
    public enum ConfigLevel
    {
        /// <summary>
        /// Use a <c>.netconfig.user</c> file, instead of the default <c>.netconfig</c>, 
        /// which allows separating local-only settings from potentially 
        /// team-wide configuration files that can be checked-in source control.
        /// </summary>
        Local,

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
