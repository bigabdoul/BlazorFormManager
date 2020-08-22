using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace BlazorFormManager
{
    internal static class TypeExtensions
    {
        private const string DateFormat = "yyyy-MM-dd"; // Compatible with HTML date inputs

        public static string GenerateId(this Type type) => $"{type.Name}_{Guid.NewGuid().GetHashCode():X}";
        public static string GenerateId(this string name) => $"{name}_{Guid.NewGuid().GetHashCode():X}";

        public static IEnumerable<SelectOption> OptionsFromRange(this RangeAttribute range, bool useUnderlyingEnumType = false, double step = 1.0)
        {
            if (range != null)
            {
                if (range.Minimum is int imin && range.Maximum is int imax)
                {
                    return OptionsFromRange(imin, imax);
                }
                else if (range.Minimum is double dmin && range.Maximum is double dmax)
                {
                    return OptionsFromRange(dmin, dmax, step);
                }
                else if (range.Minimum is string smin && range.Maximum is string smax)
                {
                    var type = range.OperandType;
                    if (type.IsEnum)
                    {
                        return OptionsFromEnumRange(type, smin, smax, useUnderlyingEnumType);
                    }
                    else if (range.OperandType == typeof(DateTime))
                    {
                        return OptionsFromDateRange(smin, smax);
                    }
                }
                throw new NotSupportedException($"Range type not supported.");
            }
            return Enumerable.Empty<SelectOption>();
        }

        public static IEnumerable<SelectOption> OptionsFromRange(double min, double max, double step = 1.0)
        {
            if (min > max) (max, min) = (min, max);

            yield return new SelectOption { Id = $"{min}", Value = $"{min}" };

            var current = min + step;

            while (current <= max)
            {
                yield return new SelectOption { Id = $"{current}", Value = $"{current}" };
                current += step;
            }
        }

        public static IEnumerable<SelectOption> OptionsFromRange(int min, int max)
        {
            return Enumerable.Range(min, max - min + 1).Select(n => new SelectOption
            {
                Id = $"{n}",
                Value = $"{n}",
            });
        }

        public static IEnumerable<SelectOption> OptionsFromDateRange(string min, string max, CultureInfo convertCulture = null, double step = 1.0)
        {
            if (convertCulture is null) convertCulture = CultureInfo.CurrentCulture;

            var dateMin = DateTime.Parse(min, convertCulture);
            var dateMax = DateTime.Parse(max, convertCulture);
            var dateCurrent = dateMin.AddDays(1);

            yield return new SelectOption { Id = $"{dateMin}", Value = $"{dateMin}" };

            while (dateCurrent <= dateMax)
            {
                yield return new SelectOption { Id = $"{dateCurrent}", Value = $"{dateCurrent}" };
                dateCurrent = dateCurrent.AddDays(step);
            }
        }

        public static IEnumerable<SelectOption> OptionsFromEnumRange(this Type type, string min, string max, bool useUnderlyingType = false)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (!type.IsEnum) throw new ArgumentException($"{type} must be an enumeration.");

            var evalues = Enum.GetValues(type);
            var conversionType = useUnderlyingType ? type.GetEnumUnderlyingType() : null;

            for (int i = 0; i < evalues.Length; i++)
            {
                var eval = evalues.GetValue(i);
                var ename = Enum.GetName(type, eval);

                if (ename == min)
                {
                    if (useUnderlyingType)
                        yield return NewOption(Convert.ChangeType(eval, conversionType), ename);
                    else
                        yield return NewOption(eval, ename);

                    for (int k = i + 1; k < evalues.Length; k++)
                    {
                        eval = evalues.GetValue(k);
                        ename = Enum.GetName(type, eval);

                        if (useUnderlyingType)
                            yield return NewOption(Convert.ChangeType(eval, conversionType), ename);
                        else
                            yield return NewOption(eval, ename);

                        if (ename == max) yield break;
                    }
                }
            }

            SelectOption NewOption(object id, string value) => new SelectOption { Id = $"{id}", Value = value };
        }

        public static bool IsNumeric(this object obj) => true == obj?.GetType().IsNumeric();

        public static bool IsNumeric(this Type type)
        {
            if (type is null) return false;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }


        public static bool SupportsCheckbox(this Type propertyType, string elementType)
        {
            return propertyType == typeof(bool) && (elementType == "checkbox" || string.IsNullOrWhiteSpace(elementType));
        }

        public static bool SupportsInputNumber(this Type targetType)
        {
            return targetType == typeof(int) ||
                    targetType == typeof(long) ||
                    targetType == typeof(short) ||
                    targetType == typeof(float) ||
                    targetType == typeof(double) ||
                    targetType == typeof(decimal);
        }

        public static bool IsString(this Type t) => t == typeof(string);
        public static bool IsByte(this Type t) => t == typeof(byte);
        public static bool IsSByte(this Type t) => t == typeof(sbyte);
        public static bool IsChar(this Type t) => t == typeof(char);
        public static bool IsInt16(this Type t) => t == typeof(short);
        public static bool IsUInt16(this Type t) => t == typeof(ushort);
        public static bool IsInt32(this Type t) => t == typeof(int);
        public static bool IsUInt32(this Type t) => t == typeof(uint);
        public static bool IsInt64(this Type t) => t == typeof(long);
        public static bool IsUInt64(this Type t) => t == typeof(ulong);
        public static bool IsSingle(this Type t) => t == typeof(float);
        public static bool IsDouble(this Type t) => t == typeof(double);
        public static bool IsDecimal(this Type t) => t == typeof(decimal);
        public static bool IsBoolean(this Type t) => t == typeof(bool);
        public static bool IsDate(this Type t) => t.IsDateTime() || t.IsDateTimeOffset();
        public static bool IsDateTime(this Type t) => t == typeof(DateTime);
        public static bool IsDateTimeOffset(this Type t) => t == typeof(DateTimeOffset);

        public static bool TryParseByte(this Type t, string value, out byte result) 
            => t.TryParseByte(value, NumberStyles.None, null, out result);

        public static bool TryParseByte(this Type t, string value, NumberStyles style, IFormatProvider provider, out byte result)
        {
            result = default;
            return t.IsByte() && byte.TryParse(value, style, provider, out result);
        }

        public static bool TryParseSByte(this Type t, string value, out sbyte result) 
            => t.TryParseSByte(value, NumberStyles.None, null, out result);

        public static bool TryParseSByte(this Type t, string value, NumberStyles style, IFormatProvider provider, out sbyte result)
        {
            result = default;
            return t.IsSByte() && sbyte.TryParse(value, style, provider, out result);
        }

        public static bool TryParseChar(this Type t, string value, out char result)
        {
            result = default;
            return t.IsChar() && char.TryParse(value, out result);
        }

        public static bool TryParseInt16(this Type t, string value, out short result) 
            => t.TryParseInt16(value, NumberStyles.None, null, out result);

        public static bool TryParseInt16(this Type t, string value, NumberStyles style, IFormatProvider provider, out short result)
        {
            result = default;
            return t.IsInt16() && short.TryParse(value, style, provider, out result);
        }

        public static bool TryParseUInt16(this Type t, string value, out ushort result) 
            => t.TryParseUInt16(value, NumberStyles.None, null, out result);

        public static bool TryParseUInt16(this Type t, string value, NumberStyles style, IFormatProvider provider, out ushort result)
        {
            result = default;
            return t.IsUInt16() && ushort.TryParse(value, style, provider, out result);
        }

        public static bool TryParseInt32(this Type t, string value, out int result) 
            => t.TryParseInt32(value, NumberStyles.None, null, out result);

        public static bool TryParseInt32(this Type t, string value, NumberStyles style, IFormatProvider provider, out int result)
        {
            result = default;
            return t.IsInt32() && int.TryParse(value, style, provider, out result);
        }

        public static bool TryParseUInt32(this Type t, string value, out uint result) 
            => t.TryParseUInt32(value, NumberStyles.None, null, out result);

        public static bool TryParseUInt32(this Type t, string value, NumberStyles style, IFormatProvider provider, out uint result)
        {
            result = default;
            return t.IsUInt32() && uint.TryParse(value, style, provider, out result);
        }

        public static bool TryParseInt64(this Type t, string value, out long result) 
            => t.TryParseInt64(value, NumberStyles.None, null, out result);

        public static bool TryParseInt64(this Type t, string value, NumberStyles style, IFormatProvider provider, out long result)
        {
            result = default;
            return t.IsInt64() && long.TryParse(value, style, provider, out result);
        }

        public static bool TryParseUInt64(this Type t, string value, out ulong result) 
            => t.TryParseUInt64(value, NumberStyles.None, null, out result);

        public static bool TryParseUInt64(this Type t, string value, NumberStyles style, IFormatProvider provider, out ulong result)
        {
            result = default;
            return t.IsUInt64() && ulong.TryParse(value, style, provider, out result);
        }

        public static bool TryParseSingle(this Type t, string value, out float result) 
            => t.TryParseSingle(value, NumberStyles.None, null, out result);

        public static bool TryParseSingle(this Type t, string value, NumberStyles style, IFormatProvider provider, out float result)
        {
            result = default;
            return t.IsSingle() && float.TryParse(value, style, provider, out result);
        }

        public static bool TryParseDouble(this Type t, string value, out double result) 
            => t.TryParseDouble(value, NumberStyles.None, null, out result);

        public static bool TryParseDouble(this Type t, string value, NumberStyles style, IFormatProvider provider, out double result)
        {
            result = default;
            return t.IsDouble() && double.TryParse(value, style, provider, out result);
        }

        public static bool TryParseDecimal(this Type t, string value, out decimal result) 
            => t.TryParseDecimal(value, NumberStyles.None, null, out result);

        public static bool TryParseDecimal(this Type t, string value, NumberStyles style, IFormatProvider provider, out decimal result)
        {
            result = default;
            return t.IsDecimal() && decimal.TryParse(value, style, provider, out result);
        }

        public static bool TryParseBoolean(this Type t, string value, out bool result)
        {
            result = default;
            return t.IsBoolean() && bool.TryParse(value, out result);
        }

        public static bool TryParseDateTime(this Type t, string value, out DateTime result) 
            => t.TryParseDateTime(value, DateFormat, CultureInfo.InvariantCulture, out result);

        public static bool TryParseDateTime(this Type t, string value, string format, CultureInfo culture, out DateTime result)
        {
            result = default;
            return t.IsDateTime() && BindConverter.TryConvertToDateTime(value, culture, format, out result);
        }

        public static bool TryParseDateTimeOffset(this Type t, string value, out DateTimeOffset result)
            => t.TryParseDateTimeOffset(value, DateFormat, CultureInfo.InvariantCulture, out result);

        public static bool TryParseDateTimeOffset(this Type t, string value, string format, CultureInfo culture, out DateTimeOffset result)
        {
            result = default;
            return t.IsDateTimeOffset() && BindConverter.TryConvertToDateTimeOffset(value, culture, format, out result);
        }
    }
}
