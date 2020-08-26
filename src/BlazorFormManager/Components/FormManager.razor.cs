using BlazorFormManager.ComponentModel.ViewAnnotations;
using BlazorFormManager.Debugging;
using BlazorFormManager.IO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BlazorFormManager.Components
{
    /// <summary>
    /// Provides core functionalities for handling AJAX form submissions
    /// with zero or more files, and report back data upload progress.
    /// </summary>
    public abstract class FormManagerBase : ComponentBase, IDisposable
    {
        #region private fields & properties

        private bool _scriptInitialized;
        private bool _initializingScript;
        private ConsoleLogLevel _logLevel;
        FormManagerSubmitResult _submitResult;
        private DotNetObjectReference<FormManagerBaseJSInvokable> _thisObjRef;

        [Inject] private IJSRuntime JS { get; set; }

        #endregion

        #region public properties

        #region parameters

        /// <summary>
        /// Gets or sets the form identifier as found in the rendered DOM.
        /// </summary>
        [Parameter]
        public string FormId { get; set; } = typeof(FormManagerBase).GenerateId();

        /// <summary>
        /// Gets or sets the form 'action' attribute.
        /// </summary>
        [Parameter]
        public string FormAction { get; set; }

        /// <summary>
        /// Gets or sets the form 'method' attribute. Defaults to 'post'.
        /// </summary>
        [Parameter]
        public string FormMethod { get; set; } = "post";

        /// <summary>
        /// Gets or sets the console log level.
        /// </summary>
        [Parameter]
        public virtual ConsoleLogLevel LogLevel
        {
            get => _logLevel;
            set
            {
                if (value != _logLevel)
                {
                    _logLevel = value;
                    _ = SetLogLevelAsync();
                }
            }
        }

        /// <summary>
        /// Gets or sets the child content to render within a &lt;form> tag.
        /// </summary>
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Gets or sets custom HTML attributes to render.
        /// </summary>
        [Parameter]
        public Dictionary<string, object> AdditionalAttributes { get; set; }

        /// <summary>
        /// Delegate for the <see cref="EditForm.OnValidSubmit"/> event callback.
        /// </summary>
        [Parameter]
        public EventCallback<EditContext> OnValidSubmit { get; set; }

        /// <summary>
        /// Delegate for the <see cref="EditForm.OnInvalidSubmit"/> event callback.
        /// </summary>
        [Parameter]
        public EventCallback<EditContext> OnInvalidSubmit { get; set; }

        /// <summary>
        /// Event handler invoked when the form has been submitted, regardless of success or failure.
        /// </summary>
        [Parameter]
        public EventCallback<FormManagerSubmitResult> OnSubmitDone { get; set; }

        /// <summary>
        /// Event handler invoked when a change in the data upload process occurs.
        /// </summary>
        [Parameter]
        public EventCallback<UploadProgressChangedEventArgs> OnUploadProgressChanged { get; set; }

        /// <summary>
        /// Event handler invoked when the user's navigator does not support AJAX upload with progress report.
        /// </summary>
        [Parameter]
        public EventCallback<AjaxUploadNotSupportedEventArgs> OnAjaxUploadNotSupported { get; set; }

        /// <summary>
        /// Event handler invoked when the BlazorFormManager.js script has been successfully initialized.
        /// </summary>
        [Parameter]
        public EventCallback<FormManagerBase> OnAfterScriptInitialized { get; set; }

        /// <summary>
        /// Event handler invoked before the form is submitted. If the form has validation errors
        /// or if the action has different requirements than submitting the form. this is the
        /// opportunity to intercept and cancel the form submission by setting the 
        /// <see cref="CancelEventArgs.Cancel"/> property value to true.
        /// </summary>
        [Parameter]
        public EventCallback<CancelEventArgs> OnBeforeSubmit { get; set; }

        /// <summary>
        /// Event handler invoked when the model to be submitted is requested.
        /// </summary>
        [Parameter]
        public EventCallback<ModelRequestedEventArgs> OnModelRequested { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of key-value pairs of request 
        /// headers to set before sending via XMLHttpRequest.
        /// </summary>
        [Parameter]
        public IDictionary<string, object> RequestHeaders { get; set; }

        /// <summary>
        /// Gets or sets the debug options.
        /// </summary>
        [Parameter]
        public FormManagerXhrDebug DebugOptions { get; set; } = new FormManagerXhrDebug { HtmlDocViewEnabled = true };

        /// <summary>
        /// Gets or sets the default message to display when the form was successfully submitted.
        /// </summary>
        [Parameter]
        public string SuccessMessage { get; set; } = "Form submitted successfully.";

        /// <summary>
        /// Gets or sets the default message to display when the form was not successfully submitted.
        /// </summary>
        [Parameter]
        public string ErrorMessage { get; set; } = "An error occurred while uploading the form data.";

        /// <summary>
        /// Gets or sets a value that indicates whether the progress bar should be 
        /// displayed when an upload with files is in progress.
        /// </summary>
        [Parameter]
        public bool EnableProgressBar { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that indicates whether specifying a non-null reference
        /// model is required when the <see cref="OnModelRequested"/> event is invoked. 
        /// If this parameter's value is true and no valid model is provided after the 
        /// <see cref="OnModelRequested"/> event has been invoked, the form submission 
        /// will be cancelled.
        /// </summary>
        [Parameter]
        public bool RequireModel { get; set; }

        /// <summary>
        /// Gets or sets a function callback used to retrieve a collection of 
        /// <see cref="SelectOption"/> items to use for a 'select' element or
        /// an input of type 'radio'.
        /// </summary>
        [Parameter] public Func<string, IEnumerable<SelectOption>> OptionsGetter { get; set; }

        /// <summary>
        /// Gets or sets a function callback used to dynamically retrieve the disabled state of an input.
        /// </summary>
        [Parameter] public Func<string, bool?> DisabledGetter { get; set; }

        /// <summary>
        /// Indicates whether field changes should be tracked or not.
        /// </summary>
        [Parameter] public bool EnableChangeTracking { get; set; }

        /// <summary>
        /// Indicates whether to force the <see cref="EditContext.Validate"/> method to 
        /// execute when a field value is changed.
        /// </summary>
        [Parameter] public bool ValidateOnFieldChanged { get; set; }

        /// <summary>
        /// Gets or sets a callback delegate that is invoked when a field in the form changes.
        /// </summary>
        [Parameter] public EventCallback<FormFieldChangedEventArgs> OnFieldChanged { get; set; }

        #endregion

        #region read-only

        /// <summary>
        /// Indicates whether the form currently has validation errors.
        /// </summary>
        public bool HasValidationErrors { get; protected set; }

        /// <summary>
        /// Gets the XMLHttpRequest properties from the last request.
        /// </summary>
        public FormManagerXhrResult XhrResult { get; private set; }

        /// <summary>
        /// Indicates whether the <see cref="LogLevel"/> property is <see cref="ConsoleLogLevel.Debug"/>.
        /// </summary>
        public bool IsDebug => LogLevel == ConsoleLogLevel.Debug;

        /// <summary>
        /// Gets an object that holds event data related to unsupported AJAX upload with progress report.
        /// </summary>
        public AjaxUploadNotSupportedEventArgs AjaxUploadNotSupported { get; private set; }

        /// <summary>
        /// Provides access to the browser's localStorage.
        /// </summary>
        public LocalStorage LocalStorage { get; private set; }

        #endregion

        /// <summary>
        /// Gets or sets the form's submission result.
        /// </summary>
        public FormManagerSubmitResult SubmitResult
        {
            get => _submitResult;
            set
            {
                if (value != _submitResult)
                {
                    _submitResult = value;
                    StateHasChanged();
                }
            }
        }

        #endregion

        #region initializations

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnInitialized()
        {
            LocalStorage = LocalStorage.Create(JS);
            base.OnInitialized();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="firstRender"><inheritdoc/></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // Don't use firstRender for script initialization 
            // as it may fail on the first attempt.
            if (!_scriptInitialized && !_initializingScript)
            {
                _initializingScript = true;
                try
                {
                    var options = new
                    {
                        FormId,
                        LogLevel,
                        RequireModel,
                        // RequestHeaders, // can be done here but is currently set when BeforeSend(XhrResult) is invoked
                        OnGetModel = nameof(FormManagerBaseJSInvokable.OnGetModel),
                        OnBeforeSubmit = nameof(FormManagerBaseJSInvokable.OnBeforeSubmit),
                        OnBeforeSend = nameof(FormManagerBaseJSInvokable.OnBeforeSend),
                        OnSendFailed = nameof(FormManagerBaseJSInvokable.OnSendFailed),
                        OnSendSucceeded = nameof(FormManagerBaseJSInvokable.OnSendSucceeded),
                        OnUploadChanged = nameof(FormManagerBaseJSInvokable.OnUploadChanged),
                        OnFileReaderChanged = nameof(FormManagerBaseJSInvokable.OnFileReaderChanged),
                        OnFileReaderResult = nameof(FormManagerBaseJSInvokable.OnFileReaderResult),
                        OnAjaxUploadWithProgressNotSupported = nameof(FormManagerBaseJSInvokable.OnAjaxUploadWithProgressNotSupported),
                    };

                    _thisObjRef = DotNetObjectReference.Create(new FormManagerBaseJSInvokable(this));
                    _scriptInitialized = await JS.InvokeAsync<bool>("BlazorFormManager.init", options, _thisObjRef);

                    AjaxUploadNotSupported = null;

                    if (_scriptInitialized && OnAfterScriptInitialized.HasDelegate)
                        await OnAfterScriptInitialized.InvokeAsync(this);

                    _initializingScript = false;
                }
                catch
                {
                    _initializingScript = false;
                    throw;
                }

            }

            await base.OnAfterRenderAsync(firstRender);
        }

        #endregion

        #region abstract

        /// <summary>
        /// When implemented, returns the model to upload.
        /// </summary>
        /// <returns></returns>
        public abstract object GetModel();

        #endregion

        #region form validation and submission

        /// <summary>
        /// A descendant class may override this method to implement a 
        /// notification handler that listens for form field value changes.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected internal virtual void NotifyFieldChanged(FormFieldChangedEventArgs e)
        {
            if (EnableChangeTracking)
            {
                if (OnFieldChanged.HasDelegate)
                {
                    OnFieldChanged.InvokeAsync(e);
                }
                if (e.HasFile)
                {
                    var fileAttr = e.FileAttribute;
                    var imgAttr = fileAttr as ImagePreviewAttribute;

                    _ = ReadFileAsync(new
                    {
                        InputId = e.FieldId,
                        InputName = e.Field.FieldName,
                        fileAttr.Method,
                        fileAttr.Accept,
                        fileAttr.AcceptType,
                        imgAttr?.TargetElementId,
                        imgAttr?.TargetElementAttributeName,
                    });
                }
            }
        }

        /// <summary>
        /// Makes sure that the form's 'onsubmit' DOM event handler
        /// is triggered should the traditional form submission fail.
        /// </summary>
        /// <returns></returns>
        public virtual async Task SubmitFormAsync()
        {
            EnsureScriptInitialized();
            await JS.InvokeVoidAsync("BlazorFormManager.submitForm");
        }

        /// <summary>
        /// Performs internal tasks before eventually delegating the call to the <see cref="OnValidSubmit"/> handler.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual async Task HandleValidSubmit(EditContext context)
        {
            SubmitResult = null;
            HasValidationErrors = false;

            StateHasChanged();

            if (OnValidSubmit.HasDelegate) await OnValidSubmit.InvokeAsync(context);
        }

        /// <summary>
        /// Performs internal tasks before eventually delegating the call to the <see cref="OnInvalidSubmit"/> handler.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual async Task HandleInvalidSubmit(EditContext context)
        {
            SubmitResult = null;
            HasValidationErrors = true;
            StateHasChanged();

            if (OnInvalidSubmit.HasDelegate) await OnInvalidSubmit.InvokeAsync(context);
        }

        /// <summary>
        /// Handles displaying error or success messages after the submission completes.
        /// </summary>
        /// <param name="result">An object holding information about the form submission outcome.</param>
        /// <returns></returns>
        protected virtual Task HandleSubmitDoneAsync(FormManagerSubmitResult result)
        {
            Stopwatch?.Stop();
            IsRunning = false;
            StateHasChanged();
            return Task.CompletedTask;
        }

        #endregion

        #region log level

        /// <summary>
        /// Sets the log level in the BlazorFormManager.js script.
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> SetLogLevelAsync() => SetLogLevelAsync(_logLevel);

        /// <summary>
        /// Sets the log level in the BlazorFormManager.js script using the specified <paramref name="level"/>.
        /// </summary>
        /// <param name="level">The log level to set.</param>
        /// <returns></returns>
        public virtual async Task<bool> SetLogLevelAsync(ConsoleLogLevel level)
        {
            if (_scriptInitialized)
            {
                await JS.InvokeVoidAsync("BlazorFormManager.setLogLevel", level);
                _logLevel = level;
                return true;
            }
            return false;
        }

        #endregion

        #region FormManagerBaseJSInvokable accessibility

        /// <summary>
        /// When the task completes one of the following things can happen:
        /// <para>
        /// The <see cref="OnBeforeSubmit"/> event callback has a delegate:
        /// Returns a boolean value determined by the property value of an instance of the
        /// <see cref="CancelEventArgs.Cancel"/> class passed to the event callback delegate.
        /// </para>
        /// <para>
        /// The <see cref="OnBeforeSubmit"/> event callback has no delegate:
        /// Returns true if the form has validation errors, which should cancel the submission;
        /// otherwise, false is returned and the form can be safely submitted.
        /// </para>
        /// </summary>
        /// <returns></returns>
        protected internal virtual async Task<bool> OnBeforeSubmitAsync()
        {
            AjaxUploadNotSupported = null;
            if (OnBeforeSubmit.HasDelegate)
            {
                var e = new CancelEventArgs(false);
                await OnBeforeSubmit.InvokeAsync(e);
                return e.Cancel;
            }
            return HasValidationErrors;
        }

        /// <summary>
        /// When the task completes, returns the model to be submitted.
        /// If <see cref="OnModelRequested"/> has a delegate, the model
        /// is retrieved from <see cref="ModelRequestedEventArgs.Model"/>
        /// after invocation of that delegate.
        /// </summary>
        /// <returns></returns>
        protected internal virtual async Task<object> OnGetModelAsync()
        {
            object model;

            if (OnModelRequested.HasDelegate)
            {
                var e = new ModelRequestedEventArgs();
                await OnModelRequested.InvokeAsync(e);
                model = e.Model;
            }
            else
            {
                model = GetModel();
            }

            return model;
        }

        /// <summary>
        /// Event raised before the form data is sent.
        /// </summary>
        /// <param name="xhr">
        /// An object that contains information about the XMLHttpRequest 
        /// object that is currently sending the request.
        /// </param>
        /// <returns></returns>
        protected internal virtual Task OnBeforeSendAsync(FormManagerXhrResult xhr)
        {
            XhrResult = null; // invalidate the last XHR result
            if (RequestHeaders != null)
            {
                xhr.RequestHeaders = RequestHeaders;
                if (IsDebug) Console.WriteLine($"FormManager RequestHeaders set.");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Invoked when in an error occurred during the form data submission.
        /// </summary>
        /// <param name="xhr">
        /// An object that contains information about the XMLHttpRequest 
        /// object that has sent the request.
        /// </param>
        /// <param name="message">An error message that might have been returned by the server.</param>
        /// <returns></returns>
        protected internal virtual async Task OnSendFailedAsync(FormManagerXhrResult xhr, string message = null)
        {
            XhrResult = xhr;
            SubmitResult = FormManagerSubmitResult.Failed(
                xhr,
                string.IsNullOrWhiteSpace(message) ? ErrorMessage : message,
                lastUploadHadFiles
            );

            await HandleSubmitDoneAsync(SubmitResult);

            if (OnSubmitDone.HasDelegate)
                await OnSubmitDone.InvokeAsync(SubmitResult);

            StateHasChanged();
        }

        /// <summary>
        /// Invoked when the form data has been successfully uploaded.
        /// </summary>
        /// <param name="xhr">
        /// An object that contains information about the XMLHttpRequest 
        /// object that has sent the request.
        /// </param>
        /// <param name="message">A message that might have been returned by the server.</param>
        /// <returns></returns>
        protected internal virtual async Task OnSendSucceededAsync(FormManagerXhrResult xhr, string message = null)
        {
            XhrResult = xhr;
            SubmitResult = FormManagerSubmitResult.Success(
                xhr,
                string.IsNullOrWhiteSpace(message) ? SuccessMessage : message,
                lastUploadHadFiles
            );

            await HandleSubmitDoneAsync(SubmitResult);

            if (OnSubmitDone.HasDelegate)
                await OnSubmitDone.InvokeAsync(SubmitResult);

            StateHasChanged();
        }

        /// <summary>
        /// Event fired each time a change occurs in the upload process.
        /// </summary>
        /// <param name="e">An object that holds event data related to the upload.</param>
        /// <returns></returns>
        protected internal virtual async Task<bool> OnUploadChangedAsync(UploadProgressChangedEventArgs e)
        {
            HandleUploadProgressChanged(e);
            if (OnUploadProgressChanged.HasDelegate)
                await OnUploadProgressChanged.InvokeAsync(e);
            return e.Cancel;
        }

        /// <summary>
        /// Invoked when the user's browser does not support AJAX upload 
        /// with progress report. Return false to continue form submission 
        /// using full-page refresh; otherwise, true to cancel the form submission.
        /// </summary>
        /// <param name="e">The event data.</param>
        /// <returns></returns>
        protected internal virtual async Task<bool> OnAjaxUploadWithProgressNotSupportedAsync(AjaxUploadNotSupportedEventArgs e)
        {
            AjaxUploadNotSupported = e;

            if (IsDebug)
                Console.WriteLine($"AJAX upload with progress not supported.");

            if (OnAjaxUploadNotSupported.HasDelegate)
                await OnAjaxUploadNotSupported.InvokeAsync(e);

            StateHasChanged();
            return e.Cancel;
        }

        /// <summary>
        /// For debugging purposes only. Invokes the 'raiseAjaxUploadWithProgressNotSupported'
        /// JavaScript function in BlazorFormManager.js file.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// BlazorFormManager.js script has not been initialized.
        /// </exception>
        public virtual async Task RaiseAjaxUploadWithProgressNotSupportedAsync()
        {
            EnsureScriptInitialized();
            Console.WriteLine($"Invoking {nameof(RaiseAjaxUploadWithProgressNotSupportedAsync)}...");
            await JS.InvokeVoidAsync("BlazorFormManager.raiseAjaxUploadWithProgressNotSupported");
            StateHasChanged();
        }

        #endregion

        #region IO

        #region fields

        private bool lastUploadHadFiles;

        /// <summary>
        /// A flag that allows aborting the current upload operation.
        /// </summary>
        protected bool AbortRequested;

        /// <summary>
        /// Indicates the current upload progress status text.
        /// </summary>
        protected string UploadStatus = "Preparing...";

        /// <summary>
        /// Indicates the current upload progress status text.
        /// </summary>
        protected string ReadStatus = "Preparing...";

        #endregion

        #region properties

        /// <summary>
        /// Gets the stop watch used to measure a form submission duration.
        /// </summary>
        public Stopwatch Stopwatch { get; private set; }

        /// <summary>
        /// Determines whether an active upload containing one or more files is in progress.
        /// </summary>
        public bool IsUploadingFiles => Progress != null && Progress.HasFiles;

        /// <summary>
        /// Determines whether an active file read operation is in progress.
        /// </summary>
        public bool IsReadingFiles => ReadProgress != null;

        /// <summary>
        /// Indicates that a form submission hasn't completed yet.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Indicates whether a form submission is taking more 
        /// milliseconds than specified by <see cref="LongRunningDelay"/>.
        /// </summary>
        public bool IsLongRunning => IsRunning
            && LongRunningDelay > 0
            && (Stopwatch?.ElapsedMilliseconds > LongRunningDelay);

        /// <summary>
        /// The number of milliseconds beyond which to consider a form submission 
        /// to be long running. Defaults to 2000 milliseconds.
        /// </summary>
        public int LongRunningDelay { get; set; } = 2000;

        /// <summary>
        /// Returns true if <see cref="EnableChangeTracking"/> is true and 
        /// <see cref="OnFieldChanged"/> has a delegate; otherwise, false.
        /// </summary>
        /// <returns></returns>
        public bool CanTrackChanges => EnableChangeTracking && OnFieldChanged.HasDelegate;

        /// <summary>
        /// Gets the current progress' event data.
        /// </summary>
        protected UploadProgressChangedEventArgs Progress { get; private set; }

        /// <summary>
        /// Gets the current file read progress' event data.
        /// </summary>
        protected FileReaderProgressChangedEventArgs ReadProgress { get; private set; }

        #endregion

        #region methods

        /// <summary>
        /// Handles various upload-related event types.
        /// </summary>
        /// <param name="e">The event data holder.</param>
        protected virtual void HandleUploadProgressChanged(UploadProgressChangedEventArgs e)
        {
            Progress = e;

            if (IsDebug) Console.WriteLine($"File upload changed. {e.EventType} event: {e.ProgressPercentage}%");

            switch (e.EventType)
            {
                case ProgressChangedEventType.Start:
                    UploadStatus = "Preparing to upload...";
                    StartStopWatch();
                    IsRunning = true;
                    SubmitResult = null;
                    lastUploadHadFiles = e.HasFiles;
                    break;
                case ProgressChangedEventType.Progress:
                    if (AbortRequested)
                    {
                        e.Cancel = true;
                        AbortRequested = false;
                        UploadStatus = "Aborting...";
                    }
                    else
                    {
                        UploadStatus = "Uploading...";
                    }
                    break;
                case ProgressChangedEventType.Complete:
                    UploadStatus = "Done!";
                    break;
                case ProgressChangedEventType.Error:
                    UploadStatus = "An error occurred during the upload.";
                    break;
                case ProgressChangedEventType.Abort:
                    UploadStatus = "Operation aborted.";
                    break;
                case ProgressChangedEventType.Timeout:
                    UploadStatus = "The upload timed out because a reply did not arrive " +
                        "within the time interval specified by the XMLHttpRequest.timeout.";
                    break;
                case ProgressChangedEventType.End:
                    Progress = null;
                    UploadStatus = null;
                    break;
                default:
                    break;
            }
            StateHasChanged();
        }

        #region FileReader support

        #region parameters

        /// <summary>
        /// Event handler invoked when a change in the file read process occurs.
        /// </summary>
        [Parameter]
        public EventCallback<FileReaderProgressChangedEventArgs> OnFileReaderProgressChanged { get; set; }

        /// <summary>
        /// Event handler invoked when a JavaScript file reading operation is 
        /// completed and the result is available. Check the <see cref="FileReaderResult.Succeeded"/>
        /// flag to find out if the operation completed successfully.
        /// </summary>
        [Parameter] public EventCallback<FileReaderResult> OnFileReaderResult { get; set; }

        #endregion

        /// <summary>
        /// Reads files via JavaScript found in an input tag identified in the DOM by 
        /// <paramref name="inputId"/>.
        /// </summary>
        /// <param name="inputId">The identifier of the input file to read.</param>
        /// <param name="inputName">The name of the input file.</param>
        /// <param name="method">The FileReader method to use for reading files.</param>
        /// <param name="acceptExtensions">A comma-separated list of allowed file extensions.</param>
        /// <param name="acceptFileType">The type of file allowed to be picked.</param>
        /// <param name="targetElementId">
        /// The identifier of an HTML element (typically a &lt;img /> tag) that will display the image.
        /// </param>
        /// <param name="targetElementAttributeName">
        /// The name of the target element's attribute name that will receive the base64-encoded data URL.
        /// </param>
        /// <returns></returns>
        public virtual Task<FileReaderInvokeResult> ReadFileAsync(string inputId, string inputName,
            FileReaderMethod method, string acceptExtensions = null,
            string acceptFileType = null,
            string targetElementId = null,
            string targetElementAttributeName = null)
        {
            return ReadFileAsync(new FileReaderOptions
            {
                InputId = inputId,
                InputName = inputName,
                Method = method,
                Accept = acceptExtensions,
                AcceptType = acceptFileType,
                TargetElementId = targetElementId,
                TargetElementAttributeName = targetElementAttributeName,
            });
        }

        /// <summary>
        /// Reads files via JavaScript found in an input tag identified in the DOM by 
        /// <see cref="FileReaderOptions.InputId"/>.
        /// </summary>
        /// <param name="options">An configuration object for reading files.</param>
        /// <returns></returns>
        public virtual async Task<FileReaderInvokeResult> ReadFileAsync(FileReaderOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var result = await ReadFileAsync((object)options);

            if (LogLevel >= ConsoleLogLevel.Error)
            {
                if (result.Succeeded)
                    Console.WriteLine($"File from input #{options.InputId} ({options.InputName}) has been read successfully.");
                else
                    Console.WriteLine($"Failed to read file(s) from input #{options.InputId}: {result.Error}");
            }

            return result;
        }

        /// <summary>
        /// Reads files via JavaScript found in an input tag identified in the DOM
        /// by a property defined in the given <paramref name="options"/>.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual async Task<FileReaderInvokeResult> ReadFileAsync(object options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            var result = await JS.InvokeAsync<FileReaderInvokeResult>("BlazorFormManager.readInputFile", options);
            return result;
        }

        /// <summary>
        /// Handles various file read-related event types.
        /// </summary>
        /// <param name="e">The event data holder.</param>
        protected virtual void HandleFileReaderProgressChanged(FileReaderProgressChangedEventArgs e)
        {
            ReadProgress = e;
            
            if (IsDebug) Console.WriteLine($"FileReader changed. {e.EventType} event: {e.ProgressPercentage}%");

            switch (e.EventType)
            {
                case ProgressChangedEventType.Start:
                    ReadStatus = "Preparing to read...";
                    StartStopWatch();
                    IsRunning = true;
                    StateHasChanged();
                    break;
                case ProgressChangedEventType.Progress:
                    if (AbortRequested)
                    {
                        e.Cancel = true;
                        AbortRequested = false;
                        ReadStatus = "Aborting...";
                    }
                    else
                    {
                        ReadStatus = "Reading...";
                    }
                    break;
                case ProgressChangedEventType.Complete:
                    IsRunning = false;
                    Stopwatch?.Stop();
                    ReadStatus = "Done!";
                    StateHasChanged();
                    break;
                case ProgressChangedEventType.Error:
                    ReadStatus = "An error occurred during a file read operation.";
                    break;
                case ProgressChangedEventType.Abort:
                    ReadStatus = "Operation aborted.";
                    break;
                case ProgressChangedEventType.End:
                    IsRunning = false;
                    ReadProgress = null;
                    ReadStatus = null;
                    Stopwatch?.Stop();
                    StateHasChanged();
                    break;
                default:
                    break;
            }
            StateHasChanged();
        }

        /// <summary>
        /// Event fired each time a change occurs in the file read process.
        /// </summary>
        /// <param name="e">An object that holds event data related to the read operation.</param>
        /// <returns></returns>
        protected internal async Task<bool> OnFileReaderChangedAsync(FileReaderProgressChangedEventArgs e)
        {
            HandleFileReaderProgressChanged(e);
            if (OnFileReaderProgressChanged.HasDelegate)
                await OnFileReaderProgressChanged.InvokeAsync(e);
            return e.Cancel;
        }

        /// <summary>
        /// Invokes the <see cref="OnFileReaderResult"/> event callback.
        /// </summary>
        /// <param name="result">The result of a successful file reading operation.</param>
        /// <returns></returns>
        protected internal virtual Task OnFileReaderResultAsync(FileReaderResult result)
        {
            IsRunning = false;
            return OnFileReaderResult.InvokeAsync(result);
        }

        #endregion

        #endregion

        #endregion

        #region helpers

        /// <summary>
        /// Throws <see cref="InvalidOperationException"/> if the BlazorFormManager.js
        /// script has not been initialized.
        /// </summary>
        protected virtual void EnsureScriptInitialized()
        {
            if (!_scriptInitialized)
                throw new InvalidOperationException("BlazorFormManager.js script has not been initialized.");
        }

        /// <summary>
        /// Resets and/or starts the stop watch.
        /// </summary>
        private void StartStopWatch()
        {
            if (Stopwatch == null)
                Stopwatch = new Stopwatch();
            else
                Stopwatch.Reset();
            Stopwatch.Start();
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Releases used resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release managed resources only;
        /// otherwise, false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _thisObjRef?.Dispose();
            }
        }

        #endregion
    }
}
