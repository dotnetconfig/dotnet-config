internal static class ConfigExtensions
{
    public static T Get<T>(this Config config, string section, string variable, T defaultValue = default)
        => config.Get(section, null, variable, defaultValue);
}
