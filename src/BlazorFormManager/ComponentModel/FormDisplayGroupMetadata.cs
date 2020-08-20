using System.Collections.Generic;

namespace BlazorFormManager.ComponentModel
{
    /// <summary>
    /// Holds metadata related to the way a group of inputs is layed out.
    /// </summary>
    public class FormDisplayGroupMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormDisplayGroupMetadata"/> class.
        /// </summary>
        public FormDisplayGroupMetadata()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="AutoInputMetadata"/> elements.
        /// </summary>
        public List<AutoInputMetadata> Items { get; } = new List<AutoInputMetadata>();

        /// <summary>
        /// Gets or sets the name of the section.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for a section.
        /// </summary>
        public string CssClass { get; set; }

        /// <summary>
        /// Indicates whether the section name should be displayed.
        /// </summary>
        public bool ShowName { get; set; }
    }
}