using BlazorFormManager.ComponentModel.ViewAnnotations;
using BlazorFormManager.DOM;
using BlazorFormManager.IO;
using System.Collections.Generic;

namespace BlazorFormManager.Demo.Client.Models
{
    [DisplayIgnore]
    public class DragDropOptions
    {
        [ImagePreview(Width = 150)]
        [DragDrop(Prompt = "Choose files or drop them here", Multiple = true, Accept = ".jpg, .jpeg")]
        public List<InputFileInfo> Files { get; } = new List<InputFileInfo>();

        public DragEventResponse DropResponse { get; } = new DragEventResponse { ImagePreviewOptions = new ImagePreviewOptions() };

        public bool Disabled { get; set; }
        public int FileCount { get; set; }
        public double DroppedFileSize { get; set; }
        public double UploadFileSize { get; set; }
        public bool FilesTruncated => Files.Count < FileCount;
    }
}
