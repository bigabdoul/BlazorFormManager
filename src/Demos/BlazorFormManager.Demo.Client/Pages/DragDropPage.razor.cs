using BlazorFormManager.Components.Forms;
using BlazorFormManager.Components.UI;
using BlazorFormManager.Debugging;
using BlazorFormManager.DOM;
using BlazorFormManager.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Client.Pages
{
    public partial class DragDropPage : IDisposable
    {
        #region fields & properties

        private const int PAGE_SIZE = 20;
        private bool _disposed;
        private readonly DragDropOptions Model = new DragDropOptions();

        private Exception Error { get; set; }
        private DragDropArea DragDropAreaRef { get; set; }
        private AutoEditForm<DragDropOptions> Manager { get; set; }
        private ConsoleLogLevel LogLevel => Manager?.LogLevel ?? ConsoleLogLevel.Debug;

        #endregion

        #region event handlers

        private void HandleDrop(DomDragEventArgs e)
        {
            var files = e.DataTransfer.Files;
            var count = files?.Length ?? 0;

            Model.FileCount += count;
            
            if (count > 0)
            {
                e.Response = Model.DropResponse;
                Model.DroppedFileSize += files.Sum(f => f.Size);
            }

            Manager.SubmitResult = null;
            StateHasChanged();
        }

        private void HandleReadFileList(ReadFileListEventArgs e)
        {
            switch (e.Type)
            {
                case ReadFileEventType.Start:
                    break;
                case ReadFileEventType.Rejected:
                    break;
                case ReadFileEventType.Processed:
                    Model.ProcessedFiles.Add(e.File);
                    Model.UploadFileSize += e.File.Size;
                    break;
                case ReadFileEventType.End:
                    var files = Model.Files;
                    var processedFiles = Model.ProcessedFiles;
                    
                    files.Clear();
                    StateHasChanged(); // force data pager to re-render

                    var max = processedFiles.Count > PAGE_SIZE ? PAGE_SIZE : processedFiles.Count;

                    for (int i = 0; i < max; i++)
                        files.Add(processedFiles[i]);
                    break;
                default:
                    break;
            }
            StateHasChanged();
        }

        private async Task HandleFieldChanged(FormFieldChangedEventArgs e)
        {
            if (e.IsEmptyFile(nameof(DragDropOptions.Files)))
            {
                Error = null;
                Model.ClearFiles();
                if (DragDropAreaRef != null) await DragDropAreaRef.DeleteFileListAsync();
                StateHasChanged();
            }
        }

        #endregion

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Model.ClearFiles();
                    ((IDisposable)DragDropAreaRef)?.Dispose();
                }
                _disposed = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
