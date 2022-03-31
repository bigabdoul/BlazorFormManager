namespace BlazorFormManager.Components.Web
{
    /// <summary>
    /// Encapsulates CSS styles to be applied to an HTML table.
    /// </summary>
    public class TableStyles
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableStyles"/>.
        /// </summary>
        public TableStyles()
        {
        }

        /// <summary>
        /// Gets or sets the CSS class applied to the &lt;table> element.
        /// The default value is "table".
        /// </summary>
        public string? CssClass { get; set; } = "table";

        /// <summary>
        /// Gets or sets the CSS class applied to the &lt;thead> element.
        /// </summary>
        public string? HeaderCssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS class applied to the &lt;tbody> element.
        /// </summary>
        public string? BodyCssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS class applied to the &lt;tfoot> element.
        /// </summary>
        public string? FooterCssClass { get; set; }

        /// <summary>
        /// Gets or sets the CSS class applied to the element wrapped around the 
        /// &lt;table> element. The default value is "table-responsive".
        /// </summary>
        public string? ResponsiveCssClass { get; set; } = "table-responsive";
    }
}
