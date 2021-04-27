using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace DotNetConfig
{
    static class ModuleInitializer
    {
        public static string CurrentDirectory { get; private set; } = "";

        [ModuleInitializer]
        internal static void Run() => CurrentDirectory = Directory.GetCurrentDirectory();
    }
}

#if NET472
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModuleInitializerAttribute : Attribute { }
}
#endif