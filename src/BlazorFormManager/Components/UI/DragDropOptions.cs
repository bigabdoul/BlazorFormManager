using Carfamsoft.Model2View.Annotations;
using BlazorFormManager.DOM;
using BlazorFormManager.IO;
using System.Collections.Generic;

namespace BlazorFormManager.Components.UI
{
    /// <summary>
    /// Represents an object that controls the behavior of drag &amp; drop operations.
    /// </summary>
    [DisplayIgnore]
    public class DragDropOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DragDropOptions"/> class.
        /// </summary>
        public DragDropOptions()
        {
        }

        /// <summary>
        /// Gets a list of <see cref="InputFileInfo"/> objects.
        /// </summary>
        [ImagePreview(Width = 150)]
        [DragDrop(Prompt = "Choose files or drop them here", Multiple = true, Accept = ".jpg, .jpeg")]
        public virtual List<InputFileInfo> Files { get; } = new List<InputFileInfo>();

        /// <summary>
        /// Gets a list of processed <see cref="InputFileInfo"/> objects.
        /// </summary>
        public virtual List<InputFileInfo> ProcessedFiles { get; } = new List<InputFileInfo>();
        
        /// <summary>
        /// Gets the drag event response.
        /// </summary>
        public virtual DragEventResponse DropResponse { get; } = new DragEventResponse { ImagePreviewOptions = new ImagePreviewOptions() };

        /// <summary>
        /// Indicates whether the drag &amp; drop area should be disabled.
        /// </summary>
        public virtual bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of files that can be dropped.
        /// </summary>
        public virtual int FileCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of total files dropped.
        /// </summary>
        public virtual double DroppedFileSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of total files allowed to be uploaded.
        /// </summary>
        public virtual double UploadFileSize { get; set; }

        /// <summary>
        /// Clears all file lists and initializes file count and size restrictions.
        /// </summary>
        public virtual void ClearFiles()
        {
            Files.Clear();
            ProcessedFiles.Clear();
            FileCount = 0;
            DroppedFileSize = 0d;
            UploadFileSize = 0d;
        }
    }
}
