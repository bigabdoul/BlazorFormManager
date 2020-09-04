namespace BlazorFormManager.Drawing
{
    /// <summary>
    /// Stores an ordered pair of integers, which specify a <see cref="Width"/> and <see cref="Height"/>.
    /// </summary>
    public sealed class ImageSize
    {
        /// <summary>
        /// Gets or sets the horizontal component of this <see cref="ImageSize"/> class, typically measured in pixels.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the vertical component of this <see cref="ImageSize"/> class, typically measured in pixels.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Tests whether this <see cref="ImageSize"/> class has width and height of 0.
        /// </summary>
        public bool IsEmpty => Width == 0 && Height == 0;

        /// <summary>
        /// Returns the string representation of this <see cref="ImageSize"/> class.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return IsEmpty ? string.Empty : $"{Width}x{Height}";
        }
    }
}