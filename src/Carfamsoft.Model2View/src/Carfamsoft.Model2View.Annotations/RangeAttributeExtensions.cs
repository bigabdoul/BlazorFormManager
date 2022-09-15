using Carfamsoft.Model2View.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Carfamsoft.Model2View.Annotations
{
    internal static class RangeAttributeExtensions
    {
        internal static IEnumerable<SelectOption> OptionsFromRange(this RangeAttribute range,
                                                                   bool useUnderlyingEnumType = false,
                                                                   double step = 1.0,
                                                                   Func<string, string> localizer = null)
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
                        return OptionsFromEnumRange(type, smin, smax, useUnderlyingEnumType, localizer);
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

        internal static IEnumerable<SelectOption> OptionsFromRange(double min,
                                                                   double max,
                                                                   double step = 1.0)
        {
            if (min > max)
            {
                (min, max) = (max, min);
            }

            yield return new SelectOption { Id = $"{min}", Value = $"{min}" };

            var current = min + step;

            while (current <= max)
            {
                yield return new SelectOption { Id = $"{current}", Value = $"{current}" };
                current += step;
            }
        }

        internal static IEnumerable<SelectOption> OptionsFromRange(int min, int max)
        {
            return Enumerable.Range(min, max - min + 1).Select(n => new SelectOption
            {
                Id = $"{n}",
                Value = $"{n}",
            });
        }

        internal static IEnumerable<SelectOption> OptionsFromDateRange(string min,
                                                                       string max,
                                                                       CultureInfo convertCulture = null,
                                                                       double step = 1.0)
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

        internal static IEnumerable<SelectOption> OptionsFromEnumRange(this Type type,
                                                                       string min,
                                                                       string max,
                                                                       bool useUnderlyingType = false,
                                                                       Func<string, string> localizer = null)
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

            SelectOption NewOption(object id, string value) => new SelectOption { Id = $"{id}", Value = localizer?.Invoke($"{id}") ?? value };
        }
    }
}
