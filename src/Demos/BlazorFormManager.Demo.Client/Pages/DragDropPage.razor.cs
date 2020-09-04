using BlazorFormManager.Components;
using BlazorFormManager.Debugging;
using BlazorFormManager.Demo.Client.Models;
using BlazorFormManager.DOM;
using BlazorFormManager.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorFormManager.Demo.Client.Pages
{
    public partial class DragDropPage : IDisposable
    {
        #region fields & properties

        private const int SHOW_MAX_FILES = 20;
        private bool _disposedValue;
        private readonly DragDropOptions Model = new DragDropOptions();
        private readonly List<InputFileInfo> _tempFiles = new List<InputFileInfo>();

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
                Model.Files.Clear();
                StateHasChanged();
            }
        }

        private void HandleDrop(DomDragEventArgs e)
        {
            var files = e.DataTransfer.Files;
            var count = files?.Length ?? 0;
            
            Model.FileCount = count;
            Model.UploadFileSize = 0D;

            if (count > 0)
            {
                e.Response = Model.DropResponse;
                Model.DroppedFileSize = files.Sum(f => f.Size);
            }
            else
            {
                Model.DroppedFileSize = 0D;
            }

            Manager.SubmitResult = null;
            StateHasChanged();
        }

        private void HandleReadFileList(ReadFileListEventArgs e)
        {
            switch (e.Type)
            {
                case ReadFileEventType.Start:
                    Model.Files.Clear();
                    break;
                case ReadFileEventType.Rejected:
                    break;
                case ReadFileEventType.Processed:
                    _tempFiles.Add(e.File);
                    Model.UploadFileSize += e.File.Size;
                    break;
                case ReadFileEventType.End:
                    // For performance reasons list just top SHOW_MAX_FILES of dropped
                    // files; virtual scroll may be implemented to support rendering of
                    // the rest of the dropped files.
                    var max = _tempFiles.Count > SHOW_MAX_FILES ? SHOW_MAX_FILES : _tempFiles.Count;
                    for (int i = 0; i < max; i++) Model.Files.Add(_tempFiles[i]);
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
                Model.FileCount = 0;
                Model.Files.Clear();
                _tempFiles.Clear();
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
                    _tempFiles.Clear();
                    Model.Files.Clear();
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
