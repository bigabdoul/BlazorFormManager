using BlazorFormManager.Components;
using BlazorFormManager.Debugging;
using BlazorFormManager.Demo.Client.Models;
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
        private bool _disposedValue;
        private readonly DragDropOptions Model = new DragDropOptions();

        private Exception Error { get; set; }
        private DragDropArea DragDropAreaRef { get; set; }
        private AutoEditForm<DragDropOptions> Manager { get; set; }
        private ConsoleLogLevel LogLevel => Manager?.LogLevel ?? ConsoleLogLevel.Debug;

        #endregion

        #region event handlers

        private void HandleDragStart(DomDragEventArgs e)
        {
            if (Model.Files.Count > 0)
            {
                Model.ClearFiles();
                StateHasChanged();
            }
        }

        private void HandleDrop(DomDragEventArgs e)
        {
            var files = e.DataTransfer.Files;
            var count = files?.Length ?? 0;

            Model.FileCount += count;
            
            if (count > 0)
            {
                e.Response = Model.DropResponse;
                Model.DroppedFileSize = files.Sum(f => f.Size);
            }

            Manager.SubmitResult = null;
            StateHasChanged();
        }

        private void HandleReadFileList(ReadFileListEventArgs e)
        {
            switch (e.Type)
            {
                case ReadFileEventType.Start:
                    //Model.Files.Clear();
                    //Model.ProcessedFiles.Clear();
                    break;
                case ReadFileEventType.Rejected:
                    break;
                case ReadFileEventType.Processed:
                    Model.ProcessedFiles.Add(e.File);
                    Model.UploadFileSize += e.File.Size;
                    break;
                case ReadFileEventType.End:
                    var processedFiles = Model.ProcessedFiles;
                    var max = processedFiles.Count > PAGE_SIZE ? PAGE_SIZE : processedFiles.Count;
                    for (int i = 0; i < max; i++)
                        Model.Files.Add(processedFiles[i]);
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
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Model.ClearFiles();
                    ((IDisposable)DragDropAreaRef)?.Dispose();
                }
                _disposedValue = true;
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
