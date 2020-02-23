/// <summary>
/// Specifies the level of configuration to use.
/// </summary>
internal enum ConfigLevel
{
    /// <summary>
    /// The local .netconfig of the current directory or an ancestor directory.
    /// </summary>
    Local,
    /// <summary>
    /// The global ~/.netconfig of the current user.
    /// </summary>
    Global,
    /// <summary>
    /// The system wide .netconfig.
    /// </summary>
    System,
}
