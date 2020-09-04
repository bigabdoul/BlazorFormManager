namespace BlazorFormManager.DOM
{
    /// <summary>
    /// Represents an object used to provide data as a response to a 'drag' DOM event.
    /// </summary>
    public sealed class DragEventResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DragEventResponse"/> class.
        /// </summary>
        public DragEventResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DragEventResponse"/> class
        /// with the <see cref="Cancel"/> property set to the given value.
        /// </summary>
        /// <param name="cancel">true to cancel the event; otherwise, false.</param>
        public DragEventResponse(bool cancel)
        {
            Cancel = cancel;
        }

        /// <summary>
        /// Gets or sets the kinds of operations that are to be allowed.
        /// </summary>
        public EffectAllowed EffectAllowed { get; set; }

        /// <summary>
        /// Gets or sets the drag event data.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the drag event data format.
        /// </summary>
        public string DataFormat { get; set; }

        /// <summary>
        /// true to cancel the event; otherwise, false.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets an array of files accepted on a drop event.
        /// </summary>
        public string[] AcceptedFiles { get; set; }

        /// <summary>
        /// Indicates whether to store dropped files without reading their content.
        /// </summary>
        public bool StoreOnly { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of files that is allowed to be dropped.
        /// </summary>
        public int MaxFileCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum size (in megabytes) of all files allowed to be dropped.
        /// </summary>
        public double MaxTotalSize { get; set; }

        /// <summary>
        /// Gets or sets the options for dynamically generating image previews.
        /// </summary>
        public IO.ImagePreviewOptions ImagePreviewOptions { get; set; }
    }
}
