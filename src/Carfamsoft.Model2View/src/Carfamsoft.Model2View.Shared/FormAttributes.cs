using System.Collections.Generic;

namespace Carfamsoft.Model2View.Shared
{
    /// <summary>
    /// Represents an object that encapsulates form attribute values.
    /// </summary>
    public class FormAttributes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormAttributes"/> class.
        /// </summary>
        public FormAttributes()
        {
        }

        /// <summary>
        /// Gets or sets the 'id' attribute value.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the 'name' attribute value.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the 'action' attribute value.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the 'method' attribute value.
        /// </summary>
        public string Method { get; set; } = "post";

        /// <summary>
        /// Gets or sets the 'enctype' attribute value.
        /// The default value is 'multipart/form-data'.
        /// </summary>
        public string EncType { get; set; } = "multipart/form-data";

        /// <summary>
        /// Gets a collection of additional form attributes.
        /// </summary>
        public IReadOnlyDictionary<string, string> AdditionalAttributes { get; } = new Dictionary<string, string>();
    }
}
