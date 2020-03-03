using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.DotNet
{
    internal class ValueSerializer
    {
        internal static Dictionary<Type, Delegate> deserializers { get; } = new Dictionary<Type, Delegate>
        {
            { typeof(bool), new Func<string?, bool>((value) => string.IsNullOrEmpty(value) ? true : bool.Parse(value)) },
            { typeof(DateTime), new Func<string?, DateTime>((value) => DateTime.Parse(value ?? throw new ArgumentNullException(nameof(value)), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)) },
            { typeof(int), new Func<string?, int>((value) => int.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
            { typeof(long), new Func<string?, long>((value) => long.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },

            // Nullable versions
            { typeof(bool?), new Func<string?, bool?>((value) => string.IsNullOrEmpty(value) ? true : bool.Parse(value)) },
            { typeof(DateTime?), new Func<string?, DateTime?>((value) => DateTime.Parse(value ?? throw new ArgumentNullException(nameof(value)), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)) },
            { typeof(int?), new Func<string?, int?>((value) => int.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
            { typeof(long?), new Func<string?, long?>((value) => long.Parse(value ?? throw new ArgumentNullException(nameof(value)))) },
        };

        public T Deserialize<T>(string? value)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)(value ?? "");

            if (deserializers.TryGetValue(typeof(T), out var @delegate) &&
                @delegate is Func<string?, T> deserializer)
            {
                return deserializer(value);
            }
            else
            {
                throw new NotSupportedException($"Cannot deserialize value \"{value}\" to {typeof(T)}.");
            }
        }

        public string? Serialize<T>(T value)
        {
            switch (value)
            {
                case bool b:
                    return b.ToString().ToLowerInvariant();
                case int i:
                    return i.ToString();
                case long l:
                    return l.ToString();
                case string s:
                    return s;
                default:
                    return value?.ToString();
            }
        }
    }
}
