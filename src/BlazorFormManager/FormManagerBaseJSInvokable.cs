// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BlazorFormManager.Components.Forms;
using BlazorFormManager.Debugging;
using BlazorFormManager.DOM;
using BlazorFormManager.IO;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorFormManager
{
    /// <summary>
    /// Represents a wrapper around an instance of the <see cref="FormManagerBase"/>
    /// class to enable JavaScript interoperability.
    /// </summary>
    internal class FormManagerBaseJSInvokable
    {
        private readonly FormManagerBase _formManager;

        public FormManagerBaseJSInvokable(FormManagerBase component)
        {
            _formManager = component ?? throw new ArgumentNullException(nameof(component));
        }

        [JSInvokable]
        public Task<object?> OnGetModel()
        {
            return _formManager.OnGetModelAsync();
        }

        [JSInvokable]
        public Task<bool> OnBeforeSubmit()
        {
            if (_formManager.IsDebug)
                Console.WriteLine($"{nameof(OnBeforeSubmit)} invoked.");
            return _formManager.OnBeforeSubmitAsync();
        }

        [JSInvokable]
        public async Task<FormManagerXhrResult> OnBeforeSend(FormManagerXhrResult xhr)
        {
            if (_formManager.IsDebug)
                Console.WriteLine($"{nameof(OnBeforeSend)} invoked.");

            await _formManager.OnBeforeSendAsync(xhr);
            return xhr;
        }

        [JSInvokable]
        public Task OnSendSucceeded(FormManagerXhrResult xhr)
        {
            if (_formManager.IsDebug)
                Console.WriteLine($"{nameof(OnSendSucceeded)} invoked.");

            return _formManager.OnSendSucceededAsync(xhr);
        }

        [JSInvokable]
        public Task OnSendFailed(FormManagerXhrResult xhr)
        {
            var responseText = xhr.ResponseText;

            if (_formManager.LogLevel >= ConsoleLogLevel.Error)
                Console.WriteLine($"{nameof(OnSendFailed)} invoked: {responseText}");

            return _formManager.OnSendFailedAsync(xhr);
        }

        [JSInvokable]
        public Task<bool> OnUploadChanged(UploadProgressChangedEventArgs e)
            => _formManager.OnUploadChangedAsync(e);

        [JSInvokable]
        public Task<int> OnReadFileList(ReadFileListEventArgs e)
            => _formManager.OnReadFileListAsync(e);

        [JSInvokable]
        public Task<bool> OnFileReaderChanged(FileReaderProgressChangedEventArgs e)
            => _formManager.OnFileReaderChangedAsync(e);

        [JSInvokable]
        public Task OnFileReaderResult(FileReaderResult result)
            => _formManager.OnFileReaderResultAsync(result);

        [JSInvokable]
        public Task<bool> OnAjaxUploadWithProgressNotSupported(AjaxUploadNotSupportedEventArgs e)
            => _formManager.OnAjaxUploadWithProgressNotSupportedAsync(e);

        [JSInvokable]
        public Task<DragEventResponse?> OnDragStart(DomDragEventArgs e) 
            => _formManager.OnDragStartAsync(e);
        
        [JSInvokable]
        public Task<DragEventResponse?> OnDrop(DomDragEventArgs e) => _formManager.OnDropAsync(e);

        [JSInvokable]
        public Task OnReCaptchaActivity(ReCaptchaActivity activity)
        {
            if (_formManager.LogLevel >= ConsoleLogLevel.Error)
                Console.WriteLine($"{nameof(OnReCaptchaActivity)} invoked: [{activity.Type}]: {activity.Message}");

            return _formManager.OnReCaptchaActivityAsync(activity);
        }
    }
}
