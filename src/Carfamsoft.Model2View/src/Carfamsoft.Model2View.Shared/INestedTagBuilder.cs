using System;
using System.Collections.Generic;

namespace Carfamsoft.Model2View.Shared
{
    /// <summary>
    /// Defines methods and properties that are used to create nested HTML elements.
    /// </summary>
    public interface INestedTagBuilder
    {
        /// <summary>
        /// Gets a new read-only collection of <see cref="INestedTagBuilder"/> elements.
        /// </summary>
        IEnumerable<INestedTagBuilder> InnerTags { get; }

        /// <summary>
        /// Gets the tag name for this tag.
        /// </summary>
        string TagName { get; }

        /// <summary>
        /// Creates a new instance of a class that implements this interface.
        /// </summary>
        /// <param name="tagName">The name of the tag to create.</param>
        /// <returns>An initialized instance of a class that implements the 
        /// <see cref="INestedTagBuilder"/> interface.</returns>
        INestedTagBuilder Create(string tagName);

        /// <summary>
        ///  Adds a new attribute to the tag.
        /// </summary>
        /// <param name="key">The key for the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddAttribute(string key, string value);

        /// <summary>
        /// Adds an attribute to the tag if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="keyAndValue">The key and value for the attribute.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddAttributeIf(bool condition, string keyAndValue);

        /// <summary>
        /// Adds an attribute to the tag if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="key">The key for the attribute.</param>
        /// <param name="value">The value for the attribute.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddAttributeIf(bool condition, string key, string value);

        /// <summary>
        /// Adds an attribute to the tag if <paramref name="keyAndValue"/> is not empty.
        /// </summary>
        /// <param name="keyAndValue">The key and value for the attribute.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddAttributeIfNotBlank(string keyAndValue);

        /// <summary>
        /// Adds an attribute to the tag if <paramref name="value"/> is not empty.
        /// </summary>
        /// <param name="key">The key for the attribute.</param>
        /// <param name="value">The value for the attribute.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddAttributeIfNotBlank(string key, string value);

        /// <summary>
        /// Adds an initialized <see cref="INestedTagBuilder"/> to the underlying collection.
        /// </summary>
        /// <param name="tag">An initialized <see cref="INestedTagBuilder"/> to add.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddChild(INestedTagBuilder tag);

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
        INestedTagBuilder AddChildForEach<T>(IEnumerable<T> collection, Func<T, INestedTagBuilder> callback);

        /// <summary>
        /// Adds an initialized <see cref="INestedTagBuilder"/> to the underlying collection
        /// if the <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="callback">
        /// A function that creates a <see cref="INestedTagBuilder"/> if the <paramref name="condition"/> is true.
        /// </param>
        /// <returns></returns>
        INestedTagBuilder AddChildIf(bool condition, Func<INestedTagBuilder> callback);

        /// <summary>
        /// Adds an initialized <see cref="INestedTagBuilder"/> to the underlying collection
        /// if the <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="child">The child tag to add.</param>
        /// <returns></returns>
        INestedTagBuilder AddChildIf(bool condition, INestedTagBuilder child);

        /// <summary>
        /// Adds a CSS class to the list of CSS classes in the tag.
        /// </summary>
        /// <param name="name">The name of the CSS class to add.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddClass(string name);

        /// <summary>
        /// Adds a CSS class to the list of CSS classes in the tag if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="name">The name of the CSS class to add.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddClassIf(bool condition, string name);

        /// <summary>
        /// Appends the specified content to the InnerHtml property.
        /// </summary>
        /// <param name="content">The content to append.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddContent(string content);

        /// <summary>
        /// Appends the specified content to the InnerHtml
        /// property only if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to test.</param>
        /// <param name="createContentCallback">A function that creates the content to append.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddContentIf(bool condition, Func<string> createContentCallback);

        /// <summary>
        /// Appends the specified content to the InnerHtml
        /// property only if <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="condition">The condition to test.</param>
        /// <param name="content">The content to append if the <paramref name="condition"/> is true.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddContentIf(bool condition, string content);

        /// <summary>
        /// Appends the specified content to the InnerHtml
        /// property only if <paramref name="content"/> is not blank.
        /// </summary>
        /// <param name="content">The content to append</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddContentIfNotBlank(string content);

        /// <summary>
        /// Adds multiple attributes.
        /// </summary>
        /// <param name="additionalAttributes">A collection of key-value pairs representing attributes.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder AddMultipleAttributes(IEnumerable<KeyValuePair<string, object>> additionalAttributes);

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
        INestedTagBuilder ForEach<T>(IEnumerable<T> collection, Action<T, INestedTagBuilder> callback);

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
        INestedTagBuilder ForEach<T>(IEnumerable<T> collection, Action<T> callback);

        /// <summary>
        /// Sets the InnerHtml property of the element to a 
        /// non HTML-encoded version of the specified string.
        /// </summary>
        /// <param name="html">The inner HTML value for the element.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder SetInnerHtml(string html);

        /// <summary>
        /// Sets the InnerHtml property of the element to 
        /// an HTML-encoded version of the specified string.
        /// </summary>
        /// <param name="text">The string to HTML-encode.</param>
        /// <returns>A reference to this <see cref="INestedTagBuilder"/>.</returns>
        INestedTagBuilder SetText(string text);

        /// <summary>
        ///  Adds a new attribute to the tag.
        /// </summary>
        /// <param name="key">The key for the attribute.</param>
        /// <param name="value">The value of the attribute.</param>
        void MergeAttribute(string key, string value);
    }
}