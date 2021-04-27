using System.IO;

namespace DotNetConfig
{
#pragma warning disable CS0436 // ThisAssembly Type conflicts with imported type (IVT)
    static class Constants
    {
        public static string CurrentDirectory { get; } = Path.Combine(
            ThisAssembly.Project.MSBuildProjectDirectory,
            ThisAssembly.Project.OutputPath);
    }
}