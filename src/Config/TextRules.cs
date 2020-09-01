using System;
using System.Text;

namespace DotNetConfig
{
    internal static class TextRules
    {
        const long KB = 1024;

        public static void ParseKey(string key, out string section, out string? subsection, out string variable)
        {
            var parts = key.Split('.');
            if (parts.Length < 2)
                throw new ArgumentException("Expected: SECTION.[SUBSECTION.]VARIABLE");

            if (parts.Length == 2)
            {
                // NO SUBS
                var sl = Line.CreateSection(null, 0, parts[0], null);
                var vl = Line.CreateVariable(null, 1, sl.Section, null, parts[1], null);
                section = sl.Section!;
                subsection = null;
                variable = vl.Variable!;
            }
            else
            {
                var sl = Line.CreateSection(null, 0, parts[0], string.Join(".", parts[1..^1]).Trim('"'));
                var vl = Line.CreateVariable(null, 1, sl.Section, null, parts[^1], null);
                section = sl.Section!;
                subsection = sl.Subsection;
                variable = vl.Variable!;
            }
        }

        public static void ParseSection(string key, out string section, out string? subsection)
        {
            var parts = key.Split('.');

            if (parts.Length == 1)
            {
                var sl = Line.CreateSection(null, 0, parts[0], null);
                section = sl.Section!;
                subsection = null;
            }
            else
            {
                var sl = Line.CreateSection(null, 0, parts[0], string.Join(".", parts[1..^1]).Trim('"'));
                section = sl.Section!;
                subsection = sl.Subsection;
            }
        }

        public static bool ParseBoolean(string? value)
        {
            // Empty or null variable value is true for a boolean
            if (string.IsNullOrWhiteSpace(value))
                return true;

            // Regular bool parsing
            if (bool.TryParse(value, out var result))
                return result;

            // Special cases for common variants of boolean users can use.
            return value!.ToLowerInvariant() switch
            {
                "yes" or "on" or "1" => true,
                "no" or "off" or "0" => false,
                _ => throw new FormatException($"Invalid value '{value}' for a boolean. Expected yes, on, 1 or true, or no, off, 0 or false (case insensitive)."),
            };
        }

        public static long ParseNumber(string value)
        {
            // If last two are non-digits, attempt the two-letter suffix
            if (value.Length > 1 && !char.IsDigit(value[^1]))
            {
                var suffix = char.ToLowerInvariant(value[^1]);
                if (value.Length > 2 && !char.IsDigit(value[^2]))
                {
                    if (suffix != 'b')
                        throw new FormatException("Invalid number suffix. Expected k, m, g or t, or kb, mb, gb or tb (case insensitive).");

                    return long.Parse(value[..^2]) * char.ToLowerInvariant(value[^2]) switch
                    {
                        'k' => KB,
                        'm' => KB * KB,
                        'g' => KB * KB * KB,
                        't' => KB * KB * KB * KB,
                        _ => throw new FormatException("Invalid number suffix. Expected k, m, g or t, or kb, mb, gb or tb (case insensitive)."),
                    };
                }

                return long.Parse(value[..^1]) * suffix switch
                {
                    'k' => KB,
                    'm' => KB * KB,
                    'g' => KB * KB * KB,
                    't' => KB * KB * KB * KB,
                    _ => throw new FormatException("Invalid number suffix. Expected k, m, g or t, or kb, mb, gb or tb (case insensitive)."),
                };
            }

            return long.Parse(value);
        }

        public static string SerializeSubsection(string subsection)
            => subsection.Replace("\\", "\\\\").Replace("\"", "\\\"");

        public static string SerializeValue(string value)
        {
            value = value.Replace(Environment.NewLine, "\\n");

            // Escaping backslashes is applied first since it does not 
            // require adding quotes to the string.
            if (value.IndexOf('\\') != -1)
                value = value.Replace("\\", "\\\\");

            if (value.IndexOfAny(new[] { '#', ';', '"' }) == -1)
                return value;

            return "\"" + value.Trim('"').Replace("\"", "\\\"") + "\"";
        }

        public static string ToKey(string section, string? subsection, string variable)
        {
            var sb = new StringBuilder(section);
            if (subsection != null)
            {
                sb = sb.Append('.');
                if (subsection.IndexOfAny(new[] { ' ', '\\', '"', '.' }) == -1)
                    sb = sb.Append(subsection);
                else
                    sb = sb.Append("\"").Append(SerializeSubsection(subsection)).Append("\"");
            }

            return sb.Append('.').Append(variable).ToString();
        }

        public static bool TryValidateBoolean(string value, out string? error)
        {
            error = default;
            try
            {
                ParseBoolean(value);
                return true;
            }
            catch (FormatException fe)
            {
                error = fe.Message;
                return false;
            }
        }

        public static bool TryValidateNumber(string value, out string? error)
        {
            error = default;
            try
            {
                ParseNumber(value);
                return true;
            }
            catch (FormatException fe)
            {
                error = fe.Message;
                return false;
            }
        }
    }
}
