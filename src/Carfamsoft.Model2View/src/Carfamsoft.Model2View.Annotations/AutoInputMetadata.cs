using Carfamsoft.Model2View.Shared;
using Carfamsoft.Model2View.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Resources;

namespace Carfamsoft.Model2View.Annotations
{
    /// <summary>
    /// Holds metadata used to render an auto-generated HTML form element.
    /// </summary>
    public class AutoInputMetadata
    {
        #region private fields
        
        private readonly string _propertyName;
        private readonly ResourceManager _resourceManager;
        private RequiredAttribute _requiredAttr;
        private bool _requiredVisited;
        private readonly bool _isEmpty;

        #endregion

        #region constructors

        private AutoInputMetadata()
        {
            _isEmpty = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoInputMetadata"/> class using the specified parameters.
        /// </summary>
        /// <param name="attr">An object that encapsulates display-related metadata.</param>
        /// <param name="resourceManager">The resource manager used to retrieve localized strings.</param>
        public AutoInputMetadata(FormDisplayAttribute attr, ResourceManager resourceManager = null)
        {
            Attribute = attr ?? throw new ArgumentNullException(nameof(attr));
            PropertyInfo = attr.GetProperty() ?? throw new ArgumentNullException(null, $"{nameof(PropertyInfo)} cannot be null.");
            _propertyName = PropertyInfo.Name;
            _resourceManager = resourceManager;
            ExtractOptions();
        }

        #endregion

        #region public fields and properties

        /// <summary>
        /// Represents an empty <see cref="AutoInputMetadata"/> instance.
        /// </summary>
        public static readonly AutoInputMetadata Empty = new AutoInputMetadata();

        /// <summary>
        /// Indicates whether the current <see cref="AutoInputMetadata"/> is empty.
        /// </summary>
        public bool IsEmpty => _isEmpty;

        /// <summary>
        /// Gets a collection of <see cref="SelectOption"/> items for a 'select'
        /// or 'input' element of type 'radio' to generate.
        /// </summary>
        public IEnumerable<SelectOption> Options { get; private set; }

        /// <summary>
        /// Gets the information about the property.
        /// </summary>
        public PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Gets the <see cref="FormDisplayAttribute"/>.
        /// </summary>
        public FormDisplayAttribute Attribute { get; }

        /// <summary>
        /// Gets the <see cref="RequiredAttribute"/> custom attribute associated with this metadata.
        /// </summary>
        public RequiredAttribute Required
        {
            get
            {
                if (!_requiredVisited && _requiredAttr == null)
                {
                    _requiredAttr = EnsureNotEmpty(PropertyInfo.GetCustomAttribute<RequiredAttribute>(true));
                    _requiredVisited = true;
                }
                return _requiredAttr;
            }
        }

        /// <summary>
        /// Determines whether a value is required for the property associated with this metadata.
        /// </summary>
        public bool IsRequired => Required != null;

        /// <summary>
        /// Indicates whether the input should be of type checkbox.
        /// </summary>
        public bool IsInputCheckbox => EnsureNotEmpty(PropertyInfo.PropertyType.SupportsCheckbox(Attribute.UITypeHint));

        #endregion

        #region public methods

        /// <summary>
        /// Returns the display name for an input.
        /// </summary>
        /// <returns></returns>
        public virtual string GetDisplayName() => GetDisplayString(Attribute.Name ?? _propertyName);

        /// <summary>
        /// Returns a localized string for a property of the <see cref="FormDisplayAttribute"/> attribute.
        /// </summary>
        /// <param name="name">The name of the resource to retrieve.</param>
        /// <param name="culture">
        /// An object that represents the culture for which the resource is localized.
        /// </param>
        /// <returns>
        /// The value of the resource localized for the specified culture, 
        /// or null if name cannot be found in a resource set.
        /// </returns>
        public virtual string GetDisplayString(string name, System.Globalization.CultureInfo culture = null)
            => EnsureNotEmpty(_resourceManager.GetDisplayString(name, culture));

        /// <summary>
        /// Returns the property value of a specified object.
        /// </summary>
        /// <param name="obj">The object whose property value will be returned. Can be null.</param>
        /// <returns>The property value of the specified object.</returns>
        public virtual object GetValue(object obj)
            => EnsureNotEmpty(obj != null ? PropertyInfo.GetValue(obj) : null);

        /// <summary>
        /// Sets the property value of a specified object.
        /// </summary>
        /// <param name="obj">The object whose property value will be set. Can be null.</param>
        /// <param name="value">The value of the object's property to set.</param>
        public virtual void SetValue(object obj, object value)
        {
            EnsureNotEmpty();
            if (obj != null) PropertyInfo.SetValue(obj, value);
        }

        /// <summary>
        /// Returns the property name as the display name.
        /// </summary>
        /// <returns></returns>
        public virtual string PropertyAsDisplayName() => EnsureNotEmpty(ToSentence(_propertyName));

        #endregion

        #region protected methods

        /// <summary>
        /// When implemented, extracts a range of values from the custom attribute 
        /// 'RangeAttribute' using the <see cref="PropertyInfo"/> property.
        /// </summary>
        protected virtual void ExtractOptions()
        {
            EnsureNotEmpty();
            if (Attribute.UIHint.EqualsIgnoreCase("select") || Attribute.UITypeHint.EqualsIgnoreCase("radio"))
            {
                var attr = PropertyInfo.GetCustomAttribute<RangeAttribute>();
                if (attr != null)
                    Options = attr.OptionsFromRange(localizer: name => GetDisplayString(name));
                else
                    ExtractOptionsFromString();
            }
        }

        /// <summary>
        /// Transforms the specified string instance into a sentence-like output,
        /// where the first letter is an uppercase character and each following
        /// uppercase char or digit represents the beginning of new a word.
        /// </summary>
        /// <param name="s">The string to transform.</param>
        /// <returns></returns>
        protected internal static string ToSentence(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            var sb = new System.Text.StringBuilder();

            sb.Append(char.ToUpper(s[0]));

            for (int i = 1; i < s.Length; i++)
            {
                if (char.IsUpper(s[i]) || char.IsDigit(s[i]))
                    sb.Append(' ');
                sb.Append(s[i]);
            }

            return sb.ToString();
        }

        #endregion

        #region private/internal

        private void ExtractOptionsFromString()
        {
            var values = Attribute.Options;

            if (values.IsBlank()) return;

            var list = new List<SelectOption>();

            foreach (var kvp in values.ParseKeyValuePairs())
            {
                list.Add(new SelectOption
                { 
                    Id = kvp.Key, 
                    Value = GetDisplayString($"{kvp.Value}"),
                });
            }

            Options = list.ToArray();
        }

        /// <summary>
        /// Makes sure that the current <see cref="AutoInputMetadata"/> instance is not empty.
        /// </summary>
        protected virtual void EnsureNotEmpty() => EnsureNotEmpty(false);

        /// <summary>
        /// Makes sure that the current <see cref="AutoInputMetadata"/> 
        /// instance is not empty and, if not, returns the specified value.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="value">The value to return is not empty.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">The current <see cref="AutoInputMetadata"/> instance is empty.</exception>
        protected virtual T EnsureNotEmpty<T>(T value) => _isEmpty 
            ? throw new InvalidOperationException($"The current {nameof(AutoInputMetadata)} instance is empty.")
            : value;

        #endregion
    }
}
