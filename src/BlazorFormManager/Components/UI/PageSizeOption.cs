using System;

namespace BlazorFormManager.Components.UI
{
    /// <summary>
    /// Represents an object that specifies the page size of a <see cref="DataPager"/> component.
    /// </summary>
    public class PageSizeOption
    {
        private int _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageSizeOption"/> class.
        /// </summary>
        /// <param name="value">The option's value.</param>
        public PageSizeOption(int value) : this(value, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageSizeOption"/> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="value">The option's value.</param>
        /// <param name="text">The option's text. If null, <paramref name="value"/> will be used.</param>
        public PageSizeOption(int value, string? text)
        {
            Value = value;
            Text = text ?? $"{value}";
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public int Value
        {
            get => _value;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (value != _value)
                {
                    _value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string? Text { get; set; }
    }
}
