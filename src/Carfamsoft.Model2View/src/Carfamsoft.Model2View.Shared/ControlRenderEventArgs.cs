namespace Carfamsoft.Model2View.Shared
{
    /// <summary>
    /// Provides data when a control is rendered.
    /// </summary>
    public class ControlRenderEventArgs : System.ComponentModel.CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlRenderEventArgs"/> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="property">The event-related property information.</param>
        /// <param name="value">The event-related property value.</param>
        /// <param name="parentPropertyName">The name of <see cref="Property"/>'s parent.</param>
        public ControlRenderEventArgs(System.Reflection.PropertyInfo property, object value, string parentPropertyName = null)
        {
            Property = property;
            Value = value;
            ParentPropertyName = parentPropertyName;
        }

        /// <summary>
        /// Gets the name of <see cref="Property"/>'s parent.
        /// </summary>
        public string ParentPropertyName { get; }

        /// <summary>
        /// Gets the event-related property information.
        /// </summary>
        public System.Reflection.PropertyInfo Property { get; }

        /// <summary>
        /// Gets or sets the event-related property value.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the render output.
        /// </summary>
        public string RenderOutput { get; set; }

        /// <summary>
        /// Indicates whether the event was handled.
        /// </summary>
        public bool Handled { get; set; }
    }
}