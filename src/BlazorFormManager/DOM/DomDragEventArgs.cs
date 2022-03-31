namespace BlazorFormManager.DOM
{
    /// <summary>
    /// A DOM event that represents a drag and drop interaction. The user initiates a
    /// drag by placing a pointer device (such as a mouse) on the touch surface and then
    /// dragging the pointer to a new location (such as another DOM element).
    /// </summary>
    public sealed class DomDragEventArgs : DomMouseEventArgs
    {
        DomDataTransfer _dataTransfer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomDragEventArgs"/> class.
        /// </summary>
        public DomDragEventArgs()
        {
            _dataTransfer = new DomDataTransfer();
        }

        /// <summary>
        /// The data that underlies a drag-and-drop operation, known as the drag data store.
        /// </summary>
        public DomDataTransfer DataTransfer
        {
            get
            {
                if (_dataTransfer == null)
                {
                    _dataTransfer = new DomDataTransfer();
                }
                return _dataTransfer;
            }
            set
            {
                if (_dataTransfer != value)
                {
                    _dataTransfer = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the response to a drag and drop operation.
        /// </summary>
        public DragEventResponse? Response { get; set; }
    }
}
