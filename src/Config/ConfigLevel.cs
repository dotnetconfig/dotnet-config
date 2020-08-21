namespace Microsoft.DotNet
{
    /// <summary>
    /// Specifies the level of configuration to use.
    /// </summary>
    public enum ConfigLevel
    {
        /// <summary>
        /// The global ~/.netconfig for the current user.
        /// </summary>
        Global,
        
        /// <summary>
        /// The system wide .netconfig.
        /// </summary>
        System,
    }
}
