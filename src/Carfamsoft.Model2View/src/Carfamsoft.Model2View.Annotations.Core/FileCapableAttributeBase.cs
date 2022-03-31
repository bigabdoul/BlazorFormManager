namespace Carfamsoft.Model2View.Annotations
{
    /// <summary>
    /// Represents the base attribute for members capable of handling files.
    /// </summary>
    public abstract class FileCapableAttributeBase : FormAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileCapableAttributeBase"/> class.
        /// </summary>
        protected FileCapableAttributeBase()
        {
        }

        /// <summary>
        /// Gets or sets a comma-separated list of file name extensions that limits the 
        /// types of files a user can pick. If the value is null or empty (or only 
        /// whitespace) then any file can be picked.
        /// </summary>
        public virtual string Accept { get; set; }

        /// <summary>
        /// Gets or sets the type of file that can be picked up.
        /// </summary>
        public virtual string AcceptType { get; set; }

        /// <summary>
        /// Indicates whether multiple file selection is allowed.
        /// </summary>
        public virtual bool Multiple { get; set; }

        /// <summary>
        /// Gets or sets the identifier of an element that supports drag and drop.
        /// </summary>
        public virtual string DropTargetId { get; set; }

        /// <summary>
        /// Gets or sets the icon that identifies a drag and drop operation. The default is 'copy'.
        /// </summary>
        public virtual string DropEffect { get; set; } = "copy";

        /// <summary>
        /// Gets or sets the method of the FileReader API (in JavaScript) to use.
        /// </summary>
        public virtual FileReaderMethod Method { get; set; }

        /// <summary>
        /// Indicates whether an object URL should be created in JavaScript (URL.createObjectUrl() method).
        /// </summary>
        public virtual bool CreateObjectUrl { get; set; }
    }
}
