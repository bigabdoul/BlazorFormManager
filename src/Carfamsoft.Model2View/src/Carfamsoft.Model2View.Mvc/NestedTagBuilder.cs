using Carfamsoft.Model2View.Shared;
using Carfamsoft.Model2View.Shared.Extensions;
#if NETSTANDARD2_0
using Microsoft.AspNetCore.Mvc.Rendering;
#else
using System.Web.Mvc;
#endif
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Carfamsoft.Model2View.Mvc
{
    /// <summary>
    /// Contains classes and properties that are used to create nested HTML elements.
    /// </summary>
    public class NestedTagBuilder : TagBuilder, INestedTagBuilder
    {
        private readonly IList<INestedTagBuilder> _innerTags = new List<INestedTagBuilder>();

        /// <inheritdoc />
        public NestedTagBuilder(string tagName) : base(tagName)
        {
#if NETSTANDARD2_0
            TagRenderMode = HtmlTagInfo.IsSelfClosing(tagName)
                ? TagRenderMode.SelfClosing
                : TagRenderMode.Normal;
#endif
        }

        /// <summary>
        /// Gets a new read-only collection of <see cref="INestedTagBuilder"/> elements.
        /// </summary>
        public IEnumerable<INestedTagBuilder> InnerTags
        {
            get
            {
                return new ReadOnlyCollection<INestedTagBuilder>(_innerTags);
            }
        }

        /// <summary>
        /// Creates a new <see cref="INestedTagBuilder"/> class that has the specified tag name.
        /// </summary>
        /// <param name="tagName">
        /// The tag name without the "&lt;", "/", or "&gt;" delimiters.
        /// </param>
        /// <returns></returns>
        public INestedTagBuilder Create(string tagName)
        {
            return new NestedTagBuilder(tagName);
        }

        /// <summary>
        /// Adds an initialized <see cref="INestedTagBuilder"/> to the underlying collection.
        /// </summary>
        /// <param name="tag">An initialized <see cref="INestedTagBuilder"/> to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tag"/> is null.</exception>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddChild(INestedTagBuilder tag)
        {
            _innerTags.Add(tag ?? throw new ArgumentNullException(nameof(tag)));
            return this;
        }

        /// <summary>
        /// Adds an initialized <see cref="INestedTagBuilder"/> to the underlying collection
        /// if the <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="callback">
        /// A function that creates a <see cref="INestedTagBuilder"/> if the <paramref name="condition"/> is true.
        /// </param>
        /// <returns></returns>
        public INestedTagBuilder AddChildIf(bool condition, Func<INestedTagBuilder> callback)
        {
            if (condition) AddChild(callback());
            return this;
        }

        /// <summary>
        /// Adds an initialized <see cref="INestedTagBuilder"/> to the underlying collection
        /// if the <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="child">The child tag to add.</param>
        /// <returns></returns>
        public INestedTagBuilder AddChildIf(bool condition, INestedTagBuilder child)
        {
            if (condition) AddChild(child);
            return this;
        }

        /// <summary>
        /// Renders the HTML tag using a self closing or normal mode.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_innerTags.Count > 0)
            {
                SetInnerHtml(RenderSubTags(this));
            }
#if NETSTANDARD2_0
            return ToHtmlString();
#else
            return ToString(HtmlTagInfo.IsSelfClosing(TagName) ? TagRenderMode.SelfClosing : TagRenderMode.Normal);
#endif
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Renders the HTML tag.
        /// </summary>
        /// <returns>The rendered HTML tag.</returns>
        public virtual string ToHtmlString()
        {
            using (var writer = new System.IO.StringWriter())
            {
                WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
                return writer.ToString();
            }
        }
#endif

        private static string RenderSubTags(NestedTagBuilder tag)
        {
            var sb = new StringBuilder();
            foreach (var t in tag._innerTags)
            {
                sb.Append(t.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Adds multiple attributes.
        /// </summary>
        /// <param name="additionalAttributes">A collection of key-value pairs representing attributes.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddMultipleAttributes(IEnumerable<KeyValuePair<string, object>> additionalAttributes)
        {
            if (additionalAttributes != null && additionalAttributes.Any())
            {
                var culture = CultureInfo.CurrentCulture;
                foreach (var kvp in additionalAttributes)
                {
                    if (BindingConverter.TryConvertTo<string>(kvp.Value, culture, out var value))
                    {
                        MergeAttribute(kvp.Key, value);
                    }
                    else
                    {
                        MergeAttribute(kvp.Key, $"{kvp.Value}");
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Adds a CSS class to the list of CSS classes in the tag.
        /// </summary>
        /// <param name="name">The name of the CSS class to add.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddClass(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                AddCssClass(name);
            return this;
        }

        /// <summary>
        /// Adds a CSS class to the list of CSS classes in the tag if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="name">The name of the CSS class to add.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddClassIf(bool condition, string name)
        {
            if (condition && !string.IsNullOrWhiteSpace(name))
            {
                AddCssClass(name);
            }
            return this;
        }

        /// <summary>
        /// Invokes the specified <paramref name="callback"/> for each item contained in the given <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements contained in <paramref name="collection"/>.</typeparam>
        /// <param name="collection">A collection of elements for which to invoke the <paramref name="callback"/>.</param>
        /// <param name="callback">
        /// A function that creates a <see cref="INestedTagBuilder"/> for each item contained in the 
        /// <paramref name="collection"/>. If the return value for an item is null, no child tag will be added.
        /// </param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddChildForEach<T>(IEnumerable<T> collection, Func<T, INestedTagBuilder> callback)
        {
            foreach (var item in collection)
            {
                var child = callback(item);
                if (child != null) AddChild(child);
            }
            return this;
        }

        /// <summary>
        /// Invokes the specified <paramref name="callback"/> for each item contained 
        /// in the given <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements contained in <paramref name="collection"/>.</typeparam>
        /// <param name="collection">A collection of elements for which to invoke the <paramref name="callback"/>.</param>
        /// <param name="callback">
        /// A function that is invoked for each item contained in the <paramref name="collection"/>.
        /// </param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder ForEach<T>(IEnumerable<T> collection, Action<T> callback)
        {
            foreach (var item in collection)
            {
                callback(item);
            }
            return this;
        }

        /// <summary>
        /// Invokes the specified <paramref name="callback"/> for each item contained 
        /// in the given <paramref name="collection"/> and additionally passes a reference
        /// to the current <see cref="INestedTagBuilder"/> instance in the callback arguments.
        /// </summary>
        /// <typeparam name="T">The type of the elements contained in <paramref name="collection"/>.</typeparam>
        /// <param name="collection">A collection of elements for which to invoke the <paramref name="callback"/>.</param>
        /// <param name="callback">
        /// A function that is invoked for each item contained in the <paramref name="collection"/>.
        /// The second argument contains a reference to the current <see cref="INestedTagBuilder"/>.
        /// </param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder ForEach<T>(IEnumerable<T> collection, Action<T, INestedTagBuilder> callback)
        {
            foreach (var item in collection)
            {
                callback(item, this);
            }
            return this;
        }

        /// <summary>
        ///  Adds a new attribute to the tag.
        /// </summary>
        /// <param name="key">The key for the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddAttribute(string key, string value)
        {
            MergeAttribute(key, value);
            return this;
        }

        /// <summary>
        /// Adds an attribute to the tag if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="keyAndValue">The key and value for the attribute.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddAttributeIf(bool condition, string keyAndValue)
        {
            if (condition)
            {
                MergeAttribute(keyAndValue, keyAndValue);
            }
            return this;
        }

        /// <summary>
        /// Adds an attribute to the tag if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="key">The key for the attribute.</param>
        /// <param name="value">The value for the attribute.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddAttributeIf(bool condition, string key, string value)
        {
            if (condition)
            {
                MergeAttribute(key, value);
            }
            return this;
        }

        /// <summary>
        /// Adds an attribute to the tag if <paramref name="keyAndValue"/> is not empty.
        /// </summary>
        /// <param name="keyAndValue">The key and value for the attribute.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddAttributeIfNotBlank(string keyAndValue)
        {
            if (keyAndValue.IsNotBlank())
                MergeAttribute(keyAndValue, keyAndValue);
            return this;
        }

        /// <summary>
        /// Adds an attribute to the tag if <paramref name="value"/> is not empty.
        /// </summary>
        /// <param name="key">The key for the attribute.</param>
        /// <param name="value">The value for the attribute.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddAttributeIfNotBlank(string key, string value)
        {
            if (value.IsNotBlank())
                MergeAttribute(key, value);
            return this;
        }

        /// <summary>
        /// Sets an HTML-encoded string to the <see cref="TagBuilder.InnerHtml"/> property.
        /// </summary>
        /// <param name="text">The string to HTML-encode.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder SetText(string text)
        {
#if NETSTANDARD2_0
            InnerHtml.Clear().Append(text);
#else
            SetInnerText(text);
#endif
            return this;
        }

        /// <summary>
        /// Sets the <see cref="TagBuilder.InnerHtml"/> property of the 
        /// element to a non HTML-encoded version of the specified string.
        /// </summary>
        /// <param name="html">The inner HTML value for the element.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder SetInnerHtml(string html)
        {
#if NETSTANDARD2_0
            InnerHtml.Clear().AppendHtml(html);
#else
            InnerHtml = html;
#endif
            return this;
        }

        /// <summary>
        /// Appends the specified content to the <see cref="TagBuilder.InnerHtml"/> property.
        /// </summary>
        /// <param name="content">The content to append.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddContent(string content)
        {
#if NETSTANDARD2_0
            InnerHtml.AppendHtml(content);
#else
            InnerHtml += content;
#endif
            return this;
        }

        /// <summary>
        /// Appends the specified content to the <see cref="TagBuilder.InnerHtml"/> 
        /// property only if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to test.</param>
        /// <param name="createContentCallback">A function that creates the content to append.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddContentIf(bool condition, Func<string> createContentCallback)
        {
            return condition ? AddContent(createContentCallback()) : this;
        }

        /// <summary>
        /// Appends the specified content to the <see cref="TagBuilder.InnerHtml"/> 
        /// property only if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to test.</param>
        /// <param name="content">The content to append if the <paramref name="condition"/> is true.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddContentIf(bool condition, string content)
        {
            return condition ? AddContent(content) : this;
        }

        /// <summary>
        /// Appends the specified content to the <see cref="TagBuilder.InnerHtml"/> 
        /// property only if <paramref name="content"/> is not blank.
        /// </summary>
        /// <param name="content">The content to append</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        public INestedTagBuilder AddContentIfNotBlank(string content)
        {
            return content.IsNotBlank() ? AddContent(content) : this;
        }
    }
}