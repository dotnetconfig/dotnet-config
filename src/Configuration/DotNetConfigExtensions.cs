﻿using DotNetConfig;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for adding DotNetConfig support to Microsoft.Extensions.Configuration.
    /// </summary>
    public static class DotNetConfigExtensions
    {
        /// <summary>
        /// Adds the DotNetConfig configuration provider to the <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add dotnet config support to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        /// <remarks>
        /// Simply invoke this method on a builder to add hierarchical dotnet-config support 
        /// to your app, for example:
        /// <code>
        /// var config = new ConfigurationBuilder().AddDotNetConfig().Build();
        /// 
        /// var ssl = config["http:sslVerify"];
        /// </code>
        /// </remarks>
        public static IConfigurationBuilder AddDotNetConfig(this IConfigurationBuilder builder)
            => builder.Add(new DotNetConfigSource());
    }
}
