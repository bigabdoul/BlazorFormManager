namespace Carfamsoft.Model2View.Annotations
{
    /// <summary>
    /// Specifies if components should support custom rendering or not.
    /// </summary>
    public enum CustomRenderMode
    {
        /// <summary>
        /// The default value is determined by a parent setting.
        /// </summary>
        Default,

        /// <summary>
        /// Custom rendering is disabled.
        /// </summary>
        Disabled,

        /// <summary>
        /// Custom rendering is enabled.
        /// </summary>
        Enabled,
    }
}
