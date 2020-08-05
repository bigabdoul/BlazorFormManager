// Copyright (c) Karfamsoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BlazorFormManager.Components;
using BlazorFormManager.Debugging;
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
        public Task<object> OnGetModel()
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
        public async Task OnSendSucceeded(FormManagerXhrResult xhr)
        {
            if (_formManager.IsDebug)
                Console.WriteLine($"{nameof(OnSendSucceeded)} invoked.");

            await _formManager.OnSendSucceededAsync(xhr);
        }

        [JSInvokable]
        public async Task OnSendFailed(FormManagerXhrResult xhr)
        {
            var responseText = xhr?.ResponseText;

            if (_formManager.LogLevel >= ConsoleLogLevel.Error)
                Console.WriteLine($"{nameof(OnSendFailed)} invoked: {responseText}");

            await _formManager.OnSendFailedAsync(xhr);
        }

        [JSInvokable]
        public Task<bool> OnUploadChanged(UploadProgressChangedEventArgs e)
            => _formManager.OnUploadChangedAsync(e);

        [JSInvokable]
        public Task<bool> OnAjaxUploadWithProgressNotSupported(AjaxUploadNotSupportedEventArgs e)
            => _formManager.OnAjaxUploadWithProgressNotSupportedAsync(e);
    }
}
