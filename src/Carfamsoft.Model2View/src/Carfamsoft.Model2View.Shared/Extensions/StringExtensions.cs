using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Carfamsoft.Model2View.Shared.Extensions
{
    /// <summary>
    /// Contains extension methods for an instance of the <see cref="string"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Indicates whether the specified string is not null and not a System.String.Empty string after trimming.
        /// Functionally equivalent to the static method call !string.IsNullOrWhiteSpace(string).
        /// </summary>
        /// <param name="instance">The string to check.</param>
        /// <returns></returns>
        public static bool IsNotBlank(this string instance)
        {
            return !string.IsNullOrWhiteSpace(instance);
        }

        /// <summary>
        /// Indicates whether the specified string is null or a System.String.Empty string after trimming.
        /// Functionally equivalent to the static method call string.IsNullOrWhiteSpace(string).
        /// </summary>
        /// <param name="instance">The string to check.</param>
        /// <returns></returns>
        public static bool IsBlank(this string instance)
        {
            return string.IsNullOrWhiteSpace(instance);
        }

        /// <summary>
        /// Removes and replaces with the given argument anything that is NOT in the following specified set of characters: 
        /// alpha-numeric (a-zA-Z_), dash (-), and period (.)
        /// </summary>
        /// <param name="value">The value from which to remove white space.</param>
        /// <param name="replacement">The replacement string for the characters to remove.</param>
        /// <returns></returns>
        public static string ReplaceBlanks(this string value, string replacement = "-")
        {
            if (value == null)
            {
                return string.Empty;
            }

            // pattern to remove anything that is NOT in the following specified set of characters:
            //      alpha-numeric (a-zA-Z_), dash (-), and period (.)
            string pattern = @"[^\w-\.]";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            string tempName = r.Replace(value, " ");
            return string.Join(replacement, tempName.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Removes diacritics (accents) from the specified string.
        /// </summary>
        /// <param name="s">The string to alter.</param>
        public static string NoAccents(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;

            var chars = s.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();

            return new string(chars).Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Converts the specified string into words using its current title-casing.
        /// </summary>
        /// <param name="s">The string to split into words.</param>
        /// <returns>A string of one or more words.</returns>
        public static string AsTitleCaseWords(this string s)
        {
            if (s == null) return null;
            return Regex.Replace(s, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }

        /// <summary>
        /// Capitalizes all words contained in the specified string.
        /// </summary>
        /// <param name="s">The string to capitalize.</param>
        /// <returns></returns>
        public static string CapitalizeWords(this string s)
		{
            if (string.IsNullOrEmpty(s)) return s;
            return string.Join(" ", s.Split(' ').Select(str =>
            {
                if (str.Length == 1) return str.ToUpper();
                return string.Concat(str[0].ToString().ToUpper(), str.Substring(1));
			}));
		}

        /// <summary>
        /// Converts the specified string to camel case.
        /// </summary>
        /// <param name="s">The string to convert to camcel case.</param>
        /// <param name="culture">An object that supplies culture-specific casing rules.</param>
        /// <returns></returns>
        public static string AsCamelCase(this string s, CultureInfo culture = null)
        {
            if (s == null) return null;

            if (s.IndexOf('.') > -1)
                return string.Join(".", s.Split('.').Select(word => _camelCase(word)));

            return _camelCase(s);

            string _camelCase(string input)
            {
                if (input.Length == 0) return string.Empty;

                input = Regex.Replace(input, "([A-Z])([A-Z]+)($|[A-Z])", m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);

                var str = (culture != null
                    ? char.ToLower(input[0], culture)
                    : char.ToLowerInvariant(input[0])
                ).ToString();

                if (input.Length > 1) str += input.Substring(1);

                return str;
            }
        }

        /// <summary>
        /// Determines whether this string and the specified content have the same value ignoring their case by using the <see cref="StringComparison.OrdinalIgnoreCase"/> comparer.
        /// </summary>
        /// <param name="value">The value to compare against the content.</param>
        /// <param name="content">The content to compare against this string value.</param>
        /// <returns>true if the value of the value parameter is the same as this string; otherwise, false.</returns>
        public static bool EqualsIgnoreCase(this string value, string content)
        {
            if (value != null)
            {
                return value.Equals(content, StringComparison.OrdinalIgnoreCase);
            }
            else if (content == null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether a sequence contains a specified element by using a specified using the <see cref="StringComparer.OrdinalIgnoreCase"/> comparer.
        /// </summary>
        /// <param name="sequence">A sequence in which to locate a value.</param>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <returns>true if the source sequence contains an element that has the specified value; otherwise, false.</returns>
        public static bool ContainsIgnoreCase(this string[] sequence, string value)
        {
            if (sequence != null)
            {
                return sequence.Contains(value, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes \t characters in the string and trim additional space and carriage returns.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Sanitized(this string text)
        {
            if (text == null)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            var textArray = text.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in textArray.ToList())
            {
                sb.Append(item.Trim());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Removes all undesired characters in the given name and returns one that is safe and clean to use as a URL, and optionally converts it to lower case.
        /// </summary>
        /// <param name="name">The name to clean up.</param>
        /// <param name="toLower">true to convert the name to lower case; otherwise, leave it as is.</param>
        /// <returns></returns>
        public static string SanitizedName(this string name, bool toLower = true)
        {
            name = name.ReplaceBlanks().NoAccents().Sanitized();
            string[] parts = name.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            name = string.Join("-", parts);
            if (toLower)
            {
                name = name.ToLower();
            }
            return name;
        }

        /// <summary>
        /// Parses the <paramref name="values"/> into a case-insensitive string dictionary.
        /// </summary>
        /// <param name="values">A vertical pipe-separated list of key/value pairs.</param>
        /// <returns></returns>
        public static IDictionary<string, object> ParseKeyValuePairs(this string values)
        {
            var dic = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(values))
            {
                var attrs = values.Split('|').Select(s =>
                {
                    var parts = s.Trim().Split('=');
                    
                    if (parts.Length == 2)
                        return new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim());
                    else if (parts.Length == 1)
                        return new KeyValuePair<string, string>(parts[0].Trim(), parts[0].Trim());

                    throw new FormatException($"{nameof(values)} does not have the required key/value pairs format.");
                });

                foreach (var kvp in attrs)
                {
                    dic[kvp.Key] = kvp.Value;
                }
            }

            return dic;
        }

        /// <summary>
        /// Generates a unique identifier derived from the specified string.
        /// </summary>
        /// <param name="name">The name used to prefix the identifier.</param>
        /// <param name="camelCase">true to use camel-casing, otherwise false.</param>
        /// <param name="appendHashCode">true to append a random hash code to the identifier, otherwise false.</param>
        /// <returns></returns>
        public static string GenerateId(this string name, bool camelCase = false, bool appendHashCode = true) =>
            $"{(camelCase ? name.AsCamelCase() : name).Replace(".", string.Empty)}" +
            $"{(appendHashCode ? $"_{Guid.NewGuid().GetHashCode():x}" : string.Empty)}";

        /// <summary>
        /// Truncates the input string if it's longer than the specified length.
        /// </summary>
        /// <param name="input">The string to eventually truncate.</param>
        /// <param name="length">The maximum length of the string to return.</param>
        /// <param name="ellipsis">true to append 3 dots at the end of a truncated word; otherwise, false.</param>
        /// <returns></returns>
        public static string TruncateAtWord(this string input, int length, bool ellipsis = true)
        {
            if (input == null || input.Length < length)
                return input;
            int iNextSpace = input.LastIndexOf(" ", length, StringComparison.Ordinal);
            return input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim() + (ellipsis ? "..." : "");
        }

        /// <summary>
        /// Computes the hash value for the string using the specified hash format
        /// (<see cref="HashAlgorithm"/>), and optionally the provided character encoding.
        /// </summary>
        /// <param name="value">The string for which to compute the hash value.</param>
        /// <param name="hashFormat">The hash algorithm implementation to use. The following listing shows the valid values 
        /// for the hashFormat parameter: SHA, SHA1, MD5, SHA256, SHA-256, SHA384, SHA-384, SHA512, SHA-512.
        /// </param>
        /// <param name="enc">The character encoding encoding to use for the string. If null, <see cref="Encoding.UTF8"/> will be used.</param>
        /// <returns>A <see cref="string"/> that represents the computed hash value.</returns>
        public static string Hash(this string value, string hashFormat, Encoding enc = null)
        {
            var buffer = HashCore(value, hashFormat, enc);
            return enc.GetString(buffer);
        }

        /// <summary>
        /// Computes the hash value for the string using the specified hash format
        /// (<see cref="HashAlgorithm"/>), and optionally the provided character encoding,
        /// and re-encodes the result as a base64 string.
        /// </summary>
        /// <param name="value">The string for which to compute the hash value.</param>
        /// <param name="hashFormat">The hash algorithm implementation to use. The following listing shows the valid values 
        /// for the hashFormat parameter: SHA, SHA1, MD5, SHA256, SHA-256, SHA384, SHA-384, SHA512, SHA-512.
        /// </param>
        /// <param name="enc">The character encoding encoding to use for the string. If null, <see cref="Encoding.UTF8"/> will be used.</param>
        /// <returns>A base64-encoded <see cref="string"/> that represents the computed hash value.</returns>
        public static string HashBase64(this string value, string hashFormat, Encoding enc = null)
        {
            var buffer = HashCore(value, hashFormat, enc);
            return Convert.ToBase64String(buffer);
        }

        private static byte[] HashCore(string value, string hashFormat, Encoding enc = null)
        {
            if (value == null)
            {
                return null;
            }

            if (enc == null)
            {
                enc = Encoding.UTF8;
            }

            var algo = HashAlgorithm.Create(hashFormat);
            if (algo == null) throw new ArgumentException(nameof(hashFormat));

            using (algo)
            {
                byte[] buffer = enc.GetBytes(value);
                return algo.ComputeHash(buffer);
            }
        }
    }
}
