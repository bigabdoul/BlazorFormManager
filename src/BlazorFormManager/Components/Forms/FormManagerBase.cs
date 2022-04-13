using BlazorFormManager.Components.UI;
using BlazorFormManager.Debugging;
using BlazorFormManager.DOM;
using BlazorFormManager.IO;
using Carfamsoft.JSInterop;
using Carfamsoft.Model2View.Annotations;
using Carfamsoft.Model2View.Shared;
using Carfamsoft.Model2View.Shared.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace BlazorFormManager.Components.Forms
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
        private bool _disposed;
        private bool _parametersSet;
        private bool _isRunning;
        private object? _model;
        private ConsoleLogLevel _logLevel;
        FormManagerSubmitResult? _submitResult;
        private EventCallback<DomDragEventArgs> _onDragStart;
        private EventCallback<DomDragEventArgs> _onDrop;
        private DotNetObjectReference<FormManagerBaseJSInvokable>? _thisObjRef;
        private readonly EventHandler<FieldChangedEventArgs> _fieldChangedHandler;

        [Inject] private IJSRuntime? JS { get; set; }

        private const string BlazorFormManagerNS = "BlazorFormManager";
        private Timer? _stopWatchTimer;
        private readonly ElapsedEventHandler _stopWatchTimerElapsedHandler;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FormManagerBase"/> class.
        /// </summary>
        protected FormManagerBase()
        {
            _fieldChangedHandler = HandleFieldChanged;
            _stopWatchTimerElapsedHandler = (s, e) => 
                InvokeAsync(StateHasChanged);
        }

        #endregion

        #region events

        /// <summary>
        /// Event fired after the form been submitted, uploaded, 
        /// and a response is received, regardless of the outcome.
        /// </summary>
        public event Action<FormManagerSubmitResult>? Submitted;

        #endregion

        #region properties

        #region parameters

        /// <summary>
        /// Gets or sets the form identifier as found in the rendered DOM.
        /// </summary>
        [Parameter]
        public string FormId { get; set; } = typeof(FormManagerBase).Name.GenerateId();

        /// <summary>
        /// Gets or sets the form 'action' attribute.
        /// </summary>
        [Parameter]
        public string? FormAction { get; set; }

        /// <summary>
        /// Gets or sets the form 'method' attribute. Defaults to 'post'.
        /// </summary>
        [Parameter]
        public string FormMethod { get; set; } = "post";

        /// <summary>
        /// Gets or sets the form 'enctype' attribute. Defaults to 'multipart/form-data'.
        /// </summary>
        [Parameter]
        public string EncodingType { get; set; } = "multipart/form-data";

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
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the content rendered when <see cref="ContentHidden"/> is true.
        /// </summary>
        [Parameter] public RenderFragment? NotHiddenContent { get; set; }

        /// <summary>
        /// Determines whether the content of the form should be hidden.
        /// </summary>
        [Parameter] public bool ContentHidden { get; set; }

        /// <summary>
        /// Determines whether the content of the form should be hidden after a successful submission.
        /// </summary>
        [Parameter] public bool HideOnSuccess { get; set; }
        
        /// <summary>
        /// Gets or sets custom HTML attributes to render.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

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
        /// Event handler invoked when a drag action starts.
        /// </summary>
        [Parameter]
        public EventCallback<DomDragEventArgs> OnDragStart
        {
            get => _onDragStart;
            set
            {
                if (!Equals(_onDragStart, value))
                {
                    _onDragStart = value;
                    if (_scriptInitialized)
                    {
                        // enable or disable the 'dragstart' JS event callback
                        _ = UpdateScriptOptionsAsync(new
                        {
                            OnDragStart = _onDragStart.HasDelegate ? nameof(FormManagerBaseJSInvokable.OnDragStart) : null
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Event handler invoked when a drop action occurs.
        /// </summary>
        [Parameter]
        public EventCallback<DomDragEventArgs> OnDrop
        {
            get => _onDrop;
            set
            {
                if (!Equals(_onDrop, value))
                {
                    _onDrop = value;
                    if (_scriptInitialized)
                    {
                        // enable or disable the 'drop' JS event callback
                        _ = UpdateScriptOptionsAsync(new
                        {
                            OnDrop = _onDrop.HasDelegate ? nameof(FormManagerBaseJSInvokable.OnDrop) : null
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Event handler invoked when a Google reCAPTCHA activity is reported.
        /// </summary>
        [Parameter] public EventCallback<ReCaptchaActivity> OnReCaptchaActivity { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of key-value pairs of request 
        /// headers to set before sending via XMLHttpRequest.
        /// </summary>
        [Parameter]
        public IDictionary<string, object>? RequestHeaders { get; set; }

        /// <summary>
        /// Gets or sets the debug options.
        /// </summary>
        [Parameter]
        public FormManagerXhrDebug DebugOptions { get; set; } = new FormManagerXhrDebug { HtmlDocViewEnabled = true };

        /// <summary>
        /// Gets or sets the default message to display when the form was successfully submitted.
        /// </summary>
        [Parameter]
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// Gets or sets the default message to display when the form was not successfully submitted.
        /// </summary>
        [Parameter]
        public string? ErrorMessage { get; set; } = "An error occurred while submitting the form.";

        /// <summary>
        /// Gets or sets the messages and images to display when the form was submitted.
        /// </summary>
        [Parameter] public MessageDictionary? Messages { get; set; }

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
        [Parameter] public Func<string, IEnumerable<SelectOption>>? OptionsGetter { get; set; }

        /// <summary>
        /// Gets or sets a function callback used to dynamically retrieve the disabled state of an input.
        /// </summary>
        [Parameter] public Func<string, bool?>? DisabledGetter { get; set; }

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

        /// <summary>
        /// Gets or sets an object that controls a part of the HTML generation process.
        /// </summary>
        [Parameter]
        public ControlRenderOptions? RenderOptions { get; set; }

        /// <summary>
        /// If set, adds a submit button and specifies the options to use for it.
        /// </summary>
        [Parameter] public SubmitButtonOptions? SubmitButton { get; set; }

        /// <summary>
        /// Indicates whether to generate the 'name' attribute for 
        /// HTML form elements, such as input, select, textarea, etc.
        /// </summary>
        [Parameter] public bool GenerateInputNameAttribute { get; set; }

        /// <summary>
        /// Gets or sets a hidden input element (antiforgery token) 
        /// that will be validated when the form is submitted.
        /// </summary>
        [Parameter] public string? AntiForgeryToken { get; set; }

        /// <summary>
        /// If set, enables Google's reCAPTCHA technology.
        /// </summary>
        [Parameter] public ReCaptchaOptions? ReCaptcha { get; set; }

        /// <summary>
        /// Whether to enable the Quill rich text editor (https://quilljs.com).
        /// If enabled, all textarea elements will turn into rich text editors.
        /// </summary>
        [Parameter] public bool EnableRichText { get; set; }

        /// <summary>
        /// Gets or sets the culture for the current form.
        /// </summary>
        [Parameter] public System.Globalization.CultureInfo? Culture { get; set; }

        #endregion

        #region read-only

        /// <summary>
        /// Indicates whether the form currently has validation errors.
        /// </summary>
        public bool HasValidationErrors { get; protected set; }

        /// <summary>
        /// Gets the XMLHttpRequest properties from the last request.
        /// </summary>
        public FormManagerXhrResult? XhrResult { get; private set; }

        /// <summary>
        /// Indicates whether the <see cref="LogLevel"/> property is <see cref="ConsoleLogLevel.Debug"/>.
        /// </summary>
        public bool IsDebug => LogLevel == ConsoleLogLevel.Debug;

        /// <summary>
        /// Gets an object that holds event data related to unsupported AJAX upload with progress report.
        /// </summary>
        public AjaxUploadNotSupportedEventArgs? AjaxUploadNotSupported { get; private set; }

        /// <summary>
        /// Provides access to the browser's localStorage.
        /// </summary>
        public ILocalStorage? LocalStorage { get; private set; }

        #endregion

        /// <summary>
        /// Indicates whether the form has unsaved changes.
        /// </summary>
        public bool HasChanges { get; protected set; }

        /// <summary>
        /// Gets or sets the form model to upload.
        /// </summary>
        public virtual object? Model
        {
            get => _model;
            set
            {
                if (!Equals(value, _model))
                {
                    _model = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the form's submission result.
        /// </summary>
        public FormManagerSubmitResult? SubmitResult
        {
            get => _submitResult;
            set
            {
                var changed = !EqualityComparer<FormManagerSubmitResult?>.Default.Equals(value, _submitResult);
                if (changed)
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
            LocalStorage = new LocalStorage(JS!)
            {
                ApplicationName = BlazorFormManagerNS,
            };
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
                    _thisObjRef = DotNetObjectReference.Create(new FormManagerBaseJSInvokable(this));
                    var hasProgressBar = EnableProgressBar;

                    if (Culture != null && ReCaptcha != null && ReCaptcha.LanguageCode.IsBlank())
                    {
                        ReCaptcha.LanguageCode = Culture.Name;
                    }

                    // script options
                    var options = new
                    {
                        FormId,
                        LogLevel,
                        RequireModel,
                        ReCaptcha,
                        // RequestHeaders, // can be done here but is currently set when BeforeSend(XhrResult) is invoked
                        OnGetModel      = nameof(FormManagerBaseJSInvokable.OnGetModel),
                        OnBeforeSubmit  = nameof(FormManagerBaseJSInvokable.OnBeforeSubmit),
                        OnBeforeSend    = nameof(FormManagerBaseJSInvokable.OnBeforeSend),
                        OnSendFailed    = nameof(FormManagerBaseJSInvokable.OnSendFailed),
                        OnSendSucceeded = nameof(FormManagerBaseJSInvokable.OnSendSucceeded),
                        OnReCaptchaActivity = nameof(FormManagerBaseJSInvokable.OnReCaptchaActivity),
                        OnUploadChanged = hasProgressBar ? nameof(FormManagerBaseJSInvokable.OnUploadChanged) : null,

                        // opt-in file reading event handlers
                        OnReadFileList      = hasProgressBar || OnReadFileList.HasDelegate ? nameof(FormManagerBaseJSInvokable.OnReadFileList) : null,
                        OnFileReaderChanged = hasProgressBar || OnFileReaderProgressChanged.HasDelegate ? nameof(FormManagerBaseJSInvokable.OnFileReaderChanged) : null,
                        OnFileReaderResult  = OnFileReaderResult.HasDelegate ? nameof(FormManagerBaseJSInvokable.OnFileReaderResult) : null,

                        OnDragStart = nameof(FormManagerBaseJSInvokable.OnDragStart),
                        OnDrop      = nameof(FormManagerBaseJSInvokable.OnDrop),

                        OnAjaxUploadWithProgressNotSupported = nameof(FormManagerBaseJSInvokable.OnAjaxUploadWithProgressNotSupported),

                        dotNetObjectReference = _thisObjRef
                    };

                    _scriptInitialized = await JS!.InvokeAsync<bool>($"{BlazorFormManagerNS}.init", options);

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
        public abstract object? GetModel();

        /// <summary>
        /// Attempts to assign the specified value to a nested property belonging to the type of the form's object model.
        /// For better performance, it is recommended to pre-instantiate any nested (non-string) reference-type properties.
        /// </summary>
        /// <param name="navigationPath">The full navigation path to the property whose value is being set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="valueType">The type of the value to assign. Can be null.</param>
        /// <param name="throwOnError">true to throw exceptions; otherwise, false.</param>
        /// <returns>true if the operation succeeds; otherwise, false.</returns>
        public virtual bool SetModelValue(string navigationPath, object? value, Type? valueType = null, bool throwOnError = false)
        {
            try
            {
                return GetModel().SetNestedPropertyValue(navigationPath, value, valueType);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                
                if (throwOnError)
                    throw;

                return false;
            }
        }

        #endregion

        #region form validation and submission

        /// <summary>
        /// Gets or sets the form's current edit context.
        /// </summary>
        public EditContext? EditContext { get; protected set; }

        /// <summary>
        /// A descendant class may override this method to implement a 
        /// notification handler that listens for form field value changes.
        /// </summary>
        /// <param name="e">The event data.</param>
        public virtual void NotifyFieldChanged(FormFieldChangedEventArgs e)
        {
            HasChanges = true;

            if (EnableChangeTracking)
            {
                if (ValidateOnFieldChanged)
                {
                    HasValidationErrors = true == EditContext?.Validate();
                }

                EditContext?.NotifyFieldChanged(e.Field);

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
                        FormId,
                        InputId = e.FieldId,
                        InputName = e.Field.FieldName,
                        fileAttr?.Method,
                        fileAttr?.Accept,
                        fileAttr?.AcceptType,
                        fileAttr?.Multiple,
                        fileAttr?.CreateObjectUrl,
                        imagePreviewOptions = new
                        {
                            autoGenerate = imgAttr?.AutoGenerate ?? true,
                            generateFileInfo = imgAttr?.GenerateFileInfo ?? true,
                            tagClass = imgAttr?.InputCssClass,
                            tagId = imgAttr?.TargetElementId,
                            attributeName = imgAttr?.TargetElementAttributeName,
                            width = imgAttr?.Width,
                            height = imgAttr?.Height,
                            preserveAspectRatio = true,
                            noResize = false,
                        },
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
            await JS!.InvokeVoidAsync($"{BlazorFormManagerNS}.submitForm", FormId);
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
            var success = result.Succeeded;
            HasChanges = !success;
            Stopwatch?.Stop();

            if (_stopWatchTimer != null)
                _stopWatchTimer.Enabled = false;

            Progress = null;
            
            if (success && HideOnSuccess)
                ContentHidden = true;
            
            if (_isRunning)
                IsRunning = false;
            else
                StateHasChanged();

            Submitted?.Invoke(result);
            return Task.CompletedTask;
        }

        #endregion

        /// <summary>
        /// Notifies the component that its state has changed. When applicable, this will
        /// cause the component to be re-rendered. This method should only be called from
        /// a child component that modifies state that lies higher up in the hierarchy.
        /// </summary>
        public virtual Task NotifyStateChanged() => InvokeAsync(() => StateHasChanged());

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
                await JS!.InvokeVoidAsync($"{BlazorFormManagerNS}.setLogLevel", new { FormId, LogLevel = level });
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
            bool cancelled;

            SubmitResult = null;
            IsRunning = true;
            AjaxUploadNotSupported = null;

            if (OnBeforeSubmit.HasDelegate)
            {
                var e = new CancelEventArgs(false);
                await OnBeforeSubmit.InvokeAsync(e);
                cancelled = e.Cancel;
            }
            else cancelled = ValidateOnFieldChanged && HasValidationErrors;

            IsRunning = !cancelled;

            return cancelled;
        }

        /// <summary>
        /// When the task completes, returns the model to be submitted.
        /// If <see cref="OnModelRequested"/> has a delegate, the model
        /// is retrieved from <see cref="ModelRequestedEventArgs.Model"/>
        /// after invocation of that delegate.
        /// </summary>
        /// <returns></returns>
        protected internal virtual async Task<object?> OnGetModelAsync()
        {
            object? model;

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
        protected internal virtual async Task OnSendFailedAsync(FormManagerXhrResult xhr, string? message = null)
        {
            XhrResult = xhr;
            SubmitResult = FormManagerSubmitResult.Failed(
                xhr,
                string.IsNullOrWhiteSpace(message) ? Messages?.DangerOrWarning()?.Message ?? ErrorMessage : message,
                _lastUploadHadFiles
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
        protected internal virtual async Task OnSendSucceededAsync(FormManagerXhrResult xhr, string? message = null)
        {
            XhrResult = xhr;
            SubmitResult = FormManagerSubmitResult.Success(
                xhr,
                string.IsNullOrWhiteSpace(message) ? Messages?.SuccessOrInfo()?.Message ?? SuccessMessage : message,
                _lastUploadHadFiles
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
        /// Event fired when a reCAPTCHA activity is reported.
        /// </summary>
        /// <param name="activity">The activity object.</param>
        /// <returns></returns>
        protected internal virtual async Task OnReCaptchaActivityAsync(ReCaptchaActivity activity)
        {
            if (OnReCaptchaActivity.HasDelegate)
                await OnReCaptchaActivity.InvokeAsync(activity);
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
            await JS!.InvokeVoidAsync($"{BlazorFormManagerNS}.raiseAjaxUploadWithProgressNotSupported", FormId);
            StateHasChanged();
        }

        #endregion

        #region Drag and Drop

        /// <summary>
        /// Enables drag and drop support for files using the specified HTML element identifier as the drop target.
        /// </summary>
        /// <param name="dropTargetId">The identifier of the target HTML element that supports drag and drop.</param>
        /// <param name="propertyName">
        /// The name of the property used to retrieve the metadata of applicable custom attributes.
        /// This parameter is also used as the form field name into which files are stored and used when the form is submitted.
        /// </param>
        /// <param name="inputFieldId">
        /// The identifier of the input used to select a file on the disk. This value 
        /// is used to reset the input's value when a drop event occurs.
        /// </param>
        /// <returns></returns>
        public virtual ValueTask<FileReaderInvokeResult> RegisterFileDragDropTargetAsync(string dropTargetId, string propertyName, string? inputFieldId = null)
        {
            // pick up the values of which ever custom attribute first gives a clue about
            // what to accept; we'll start with the DragDropAttribute...
            var fileAttr = GetFileCapableAttributes(propertyName, out var imgAttr);
            var accept = fileAttr?.Accept ?? imgAttr?.Accept;
            var acceptType = fileAttr?.AcceptType ?? imgAttr?.AcceptType;
            var method = imgAttr?.Method ?? FileReaderMethod.ReadAsDataURL;

            // ImagePreviewAttribute-specific settings
            var autoGenerate = imgAttr?.AutoGenerate ?? true;
            object? imagePreviewOptions = imgAttr != null
                ? new
                {
                    autoGenerate,
                    generateFileInfo = imgAttr?.GenerateFileInfo ?? true,
                    tagId = autoGenerate ? null : imgAttr?.TargetElementId,
                    tagClass = imgAttr?.InputCssClass,
                    attributeName = imgAttr?.TargetElementAttributeName,
                    width = imgAttr?.Width,
                    height = imgAttr?.Height,
                    preserveAspectRatio = true,// imgAttr?.PreserveAspectRatio,
                }
                : null;

            if (string.IsNullOrWhiteSpace(dropTargetId))
                dropTargetId = fileAttr?.DropTargetId ?? $"{propertyName}{typeof(DragDropArea)}";

            return RegisterFileDragDropTargetAsync(dropTargetId, propertyName, inputFieldId
                , fileAttr?.DropEffect, accept, acceptType, fileAttr?.Multiple, method, imagePreviewOptions);
        }

        /// <summary>
        /// Enables drag and drop support for files using the specified HTML element 
        /// identifier as the drop target.
        /// </summary>
        /// <param name="dropTargetId">The identifier of the target HTML element that 
        /// supports drag and drop.</param>
        /// <param name="inputName">The store name of dropped files which is also 
        /// used when the form is submitted.</param>
        /// <param name="inputFileId">
        /// The identifier of the input used to select a file on the disk. This value 
        /// is used to reset the input's value when a drop event occurs.
        /// </param>
        /// <param name="dropEffect">The desired drop effect. Default is 'copy'.</param>
        /// <param name="accept">A comma-separated list of allowed file name extensions.</param>
        /// <param name="acceptType">A value that restricts the type of files allowed.</param>
        /// <param name="multiple">
        /// Indicates whether dropping multiple files onto the target is allowed.
        /// If null, the value from the custom attribute attached to 
        /// <paramref name="inputName"/> will be used.
        /// </param>
        /// <param name="method">
        /// The method used to read files. If a file is an image then 
        /// <see cref="FileReaderMethod.ReadAsDataURL"/> is used.
        /// </param>
        /// <param name="imagePreviewOptions">An object that specifies how image previews should be generated.</param>
        /// <returns></returns>
        public virtual ValueTask<FileReaderInvokeResult> RegisterFileDragDropTargetAsync(
            string dropTargetId
            , string inputName
            , string? inputFileId = null
            , string? dropEffect = null
            , string? accept = null
            , string? acceptType = null
            , bool? multiple = null
            , FileReaderMethod method = FileReaderMethod.ReadAsDataURL
            , object? imagePreviewOptions = null
        )
        {
            // use passed parameters; otherwise, use values from the input file attribute
            var options = new
            {
                FormId,
                dropTargetId,
                dropEffect,
                inputName,

                // No change will be triggered when the user picks a file with the input
                // file, then drags and drops a file onto the area, clicks the input file
                // again and selects the same file. Setting the 'inputFileId' parameter
                // allows resetting the corresponding input's value to an empty string
                // when a drop event occurs.
                inputFileId,

                // these properties are for file reading operations when a drop event occurs
                accept,
                acceptType,
                multiple = multiple ?? false,
                method,

                // if image preview is enabled for the property,
                // this allows it to automatically show up
                imagePreviewOptions,
            };

            return JS!.InvokeAsync<FileReaderInvokeResult>($"{BlazorFormManagerNS}.dragDropEnable", options);
        }

        /// <summary>
        /// Disables drag and drop support for the specified HTML element identifier.
        /// </summary>
        /// <param name="dropTargetId">
        /// The identifier of the target HTML element that supports drag and drop.
        /// </param>
        /// <returns></returns>
        public virtual ValueTask<FileReaderInvokeResult> UnregisterFileDragDropTargetAsync(string dropTargetId)
        {
            return JS!.InvokeAsync<FileReaderInvokeResult>($"{BlazorFormManagerNS}.dragDropDisable", dropTargetId);
        }

        /// <summary>
        /// Removes from the drag and drop storage the files that have 
        /// been previously dropped onto the specified target element.
        /// </summary>
        /// <param name="targetId">The identifier of the drop target element.</param>
        /// <returns></returns>
        public virtual ValueTask<bool> DeleteDragDropFileListAsync(string targetId)
        {
            return JS!.InvokeAsync<bool>($"{BlazorFormManagerNS}.dragDropRemoveFileList", new { FormId, targetId });
        }

        /// <summary>
        /// Removes from the processed file storage the files 
        /// that have been previously been successfully selected.
        /// </summary>
        /// <param name="inputId">The identifier of the target input element.</param>
        /// <returns></returns>
        public virtual ValueTask<bool> DeleteProcessedFileListAsync(string? inputId = null)
        {
            return JS!.InvokeAsync<bool>($"{BlazorFormManagerNS}.deleteProcessedFileList", new { FormId, inputId });
        }

        /// <summary>
        /// Reads the files from an input file identified by <paramref name="inputId"/>
        /// and stores them into the drag and drop settings for <paramref name="targetId"/>.
        /// This effectively simulates a 'drop' DOM event for the target element.
        /// </summary>
        /// <param name="inputId">The identifier of an input file element.</param>
        /// <param name="targetId">The identifier of a previously-registered drop target element.</param>
        /// <param name="propertyName">
        /// The name of the affected property. Is used to identify a concerned 
        /// property when the operation completes. This parameter is optional.
        /// </param>
        /// <param name="method">The FileReader method to use for reading files.</param>
        /// <returns></returns>
        public virtual ValueTask<FileReaderInvokeResult> DropInputFilesOnTargetAsync(string inputId, string targetId, string? propertyName = null, FileReaderMethod method = FileReaderMethod.ReadAsDataURL)
        {
            var options = new
            {
                FormId,
                targetId,
                inputId,
                InputName = propertyName,
                Method = method,
            };

            return JS!.InvokeAsync<FileReaderInvokeResult>($"{BlazorFormManagerNS}.dragDropInputFilesOnTarget", options);
        }

        /// <summary>
        /// Invoked when a drag action starts.
        /// </summary>
        /// <param name="e">An object that holds event data related to the drag action.</param>
        /// <returns></returns>
        protected internal async Task<DragEventResponse?> OnDragStartAsync(DomDragEventArgs e)
        {
            if (IsDebug) Console.WriteLine($"Drag started in {typeof(FormManagerBase)}. Event args: {e}");

            if (OnDragStart.HasDelegate)
            {
                await OnDragStart.InvokeAsync(e);
                return e.Response;
            }

            return null;
        }

        /// <summary>
        /// Invoked when a drop action occurs.
        /// </summary>
        /// <param name="e">n object that holds event data related to the drop action.</param>
        /// <returns></returns>
        protected internal async Task<DragEventResponse?> OnDropAsync(DomDragEventArgs e)
        {
            if (IsDebug) Console.WriteLine($"Dropped in {typeof(FormManagerBase)}. Event args: {e}");

            if (OnDrop.HasDelegate)
            {
                await OnDrop.InvokeAsync(e);
                return e.Response;
            }

            return null;
        }

        #endregion

        #region IO

        #region fields

        private bool _abortRequested;
        private bool _lastUploadHadFiles;
        private bool _fileListReading;
        private InputFileInfo? _currentFile;
        
        #endregion

        #region properties

        /// <summary>
        /// A flag that allows aborting the current upload operation.
        /// </summary>
        public bool AbortRequested
        {
            get => _abortRequested;
            set
            {
                if (_abortRequested != value)
                {
                    _abortRequested = value;
                    StateHasChanged();
                }
            }
        }

        /// <summary>
        /// Indicates the current upload progress status message.
        /// </summary>
        public string? UploadStatus { get; protected set; } = "Preparing...";

        /// <summary>
        /// Indicates the current file reading progress status message.
        /// </summary>
        public string? ReadStatus { get; protected set; } = "Preparing...";

        /// <summary>
        /// Indicates the current file list reading progress status message.
        /// </summary>
        public string? ReadFileListStatus { get; protected set; } = "Preparing...";

        /// <summary>
        /// Gets the upload or file read status message.
        /// </summary>
        public string? Status => IsUploadingFiles ? UploadStatus : (IsReadingFiles ? ReadStatus : string.Empty);

        /// <summary>
        /// Gets the stop watch used to measure a form submission duration.
        /// </summary>
        public Stopwatch? Stopwatch { get; private set; }

        /// <summary>
        /// Determines whether an active upload containing one or more files is in progress
        /// and the form submission is not finished yet.
        /// </summary>
        public bool IsUploadingFiles => true == Progress?.HasFiles;

        /// <summary>
        /// Determines whether an active file read operation is in progress.
        /// </summary>
        public bool IsReadingFiles => ReadProgress != null;

        /// <summary>
        /// Determines whether an active file list  reading operation is in progress.
        /// </summary>
        public bool IsReadingFileList => ReadFileListProgress != null;

        /// <summary>
        /// Indicates whether a form submission or a file reading operation is in progress.
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning; 
            private set
            {
                if (value != _isRunning)
                {
                    _isRunning = value;
                    StateHasChanged();
                }
            }
        }

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
        /// Indicates whether an active form submission can be cancelled.
        /// Returns true if an upload is ongoing; otherwise, false.
        /// </summary>
        public bool CanAbortUpload { get; private set; }

        /// <summary>
        /// Returns true if <see cref="EnableChangeTracking"/> is true and 
        /// <see cref="OnFieldChanged"/> has a delegate; otherwise, false.
        /// </summary>
        /// <returns></returns>
        public bool CanTrackChanges => EnableChangeTracking && OnFieldChanged.HasDelegate;

        /// <summary>
        /// Gets the current progress' event data.
        /// </summary>
        protected UploadProgressChangedEventArgs? Progress { get; private set; }

        /// <summary>
        /// Gets the current file list read progress event data.
        /// </summary>
        protected ReadFileListEventArgs? ReadFileListProgress { get; private set; }

        /// <summary>
        /// Gets the current file read progress' event data.
        /// </summary>
        protected FileReaderProgressChangedEventArgs? ReadProgress { get; private set; }

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
                    CanAbortUpload = true;
                    IsRunning = true;
                    AbortRequested = false;
                    SubmitResult = null;
                    _lastUploadHadFiles = e.HasFiles;
                    break;
                case ProgressChangedEventType.Progress:
                    if (AbortRequested)
                    {
                        e.Cancel = true;
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
                    CanAbortUpload = false;
                    AbortRequested = false;
                    // the server is handling the rest; clean up is done in 
                    // method HandleSubmitDoneAsync(FormManagerSubmitResult)
                    UploadStatus = "Upload done! Finalizing...";

                    // Start a timer to report stop watch updates
                    if (_stopWatchTimer != null)
                        _stopWatchTimer.Enabled = true;
                    break;
                default:
                    break;
            }
            StateHasChanged();
        }

        #region FileReader support

        #region parameters

        /// <summary>
        /// Event handler invoked when a file list reading operation occurs.
        /// </summary>
        [Parameter] public EventCallback<ReadFileListEventArgs> OnReadFileList { get; set; }

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
        /// <param name="method">The FileReader method to use for reading files.</param>
        /// <param name="inputName">The name of the input file.</param>
        /// <param name="acceptExtensions">A comma-separated list of allowed file extensions.</param>
        /// <param name="acceptFileType">The type of file allowed to be picked.</param>
        /// <param name="multiple">Indicates whether reading multiple files is allowed.</param>
        /// <param name="autoGeneratePreview">Indicates whether image previews should be automatically generated.</param>
        /// <param name="generateFileInfo">Indicates whether to include file metadata (name, size, etc.).</param>
        /// <param name="imagePreviewId">
        /// The identifier of an HTML element (typically a &lt;img /> tag) that will display the image.
        /// </param>
        /// <param name="imagePreviewAttributeName">
        /// The name of the target element's attribute name that will receive the base64-encoded data URL.
        /// </param>
        /// <param name="imagePreviewCssClass">The CSS class name for an auto-generated image preview.</param>
        /// <param name="imagePreviewWrapper">the DOM query selector for the element that is wrapped around the auto-generated image.</param>
        /// <returns></returns>
        public virtual Task<FileReaderInvokeResult> ReadFileAsync(string inputId
            , FileReaderMethod method
            , string? inputName = null
            , string? acceptExtensions = null
            , string? acceptFileType = null
            , bool multiple = false
            , bool autoGeneratePreview = true
            , bool generateFileInfo = true
            , string? imagePreviewId = null
            , string imagePreviewAttributeName = "src"
            , string? imagePreviewCssClass = null
            , string? imagePreviewWrapper = null
            )
        {
            var previewOptions = method == FileReaderMethod.ReadAsDataURL
                ? new ImagePreviewOptions
                {
                    AutoGenerate = autoGeneratePreview,
                    GenerateFileInfo = true,
                    TagId = imagePreviewId,
                    TagClass = imagePreviewCssClass,
                    AttributeName = imagePreviewAttributeName,
                    WrapperSelector = imagePreviewWrapper,
                }
                : null;
            return ReadFileAsync(new FileReaderOptions
            {
                FormId = FormId,
                InputId = inputId,
                InputName = inputName,
                Method = method,
                Accept = acceptExtensions,
                AcceptType = acceptFileType,
                Multiple = multiple,
                ImagePreviewOptions = previewOptions,
            });
            
            /* using anonymous type options
            
            if (method == FileReaderMethod.ReadAsDataURL)
            {
                var options = new
                {
                    FormId,
                    inputId,
                    inputName,
                    method,
                    accept = acceptExtensions,
                    acceptType = acceptFileType,
                    multiple,
                    imagePreviewOptions = new
                    {
                        AutoGenerate = autoGeneratePreview,
                        GenerateFileInfo = true,
                        TagId = imagePreviewId,
                        TagClass = imagePreviewCssClass,
                        AttributeName = imagePreviewAttributeName,
                        WrapperSelector = imagePreviewWrapper,
                    },
                };
                return ReadFileAsync(options);
            }
            else
            {
                var options = new
                {
                    FormId,
                    inputId,
                    inputName,
                    method,
                    aAccept = acceptExtensions,
                    acceptType = acceptFileType,
                    multiple,
                };
                return ReadFileAsync(options);
            }
            */
        }

        /// <summary>
        /// Reads files via JavaScript found in an input tag identified in the DOM by 
        /// <see cref="FileReaderOptions.InputId"/>.
        /// </summary>
        /// <param name="options">A configuration object for reading files.</param>
        /// <returns></returns>
        public virtual Task<FileReaderInvokeResult> ReadFileAsync(FileReaderOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.Method == FileReaderMethod.None) throw new InvalidOperationException("File reader method is not valid.");
            if (options.FormId.IsBlank()) options.FormId = FormId;
            return ReadFileAsync<FileReaderOptions>(options);
        }

        /// <summary>
        /// Reads files via JavaScript found in an input tag identified in the DOM
        /// by a property defined in the given <paramref name="options"/>.
        /// </summary>
        /// <param name="options">A configuration object for reading files.</param>
        /// <returns></returns>
        public virtual async Task<FileReaderInvokeResult> ReadFileAsync<T>(T options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            return await JS!.InvokeAsync<FileReaderInvokeResult>($"{BlazorFormManagerNS}.readInputFiles", options);
        }

        /// <summary>
        /// Handle various event types related to a file list reading operation.
        /// When subscribed to the corresponding event, this handler is invoked before the method
        /// <see cref="HandleFileReaderProgressChanged(FileReaderProgressChangedEventArgs)"/>.
        /// It fires at least the <see cref="ReadFileEventType.Start"/> and <see cref="ReadFileEventType.End"/>
        /// events.
        /// </summary>
        /// <param name="e">The event data holder.</param>
        protected virtual void HandleReadFileList(ReadFileListEventArgs e)
        {
            ReadFileListProgress = e;

            switch (e.Type)
            {
                case ReadFileEventType.Start:
                    _fileListReading = true;
                    IsRunning = true;
                    AbortRequested = false;
                    ReadFileListStatus = $"File list reading started with {e.TotalFilesToRead} file(s).";
                    
                    StartStopWatch();
                    StateHasChanged();
                    break;
                case ReadFileEventType.Rejected:
                    switch (e.Reason)
                    {
                        case ReadFileRejection.Extension:
                            ReadFileListStatus = $"File '{e.File!.Name}' is not allowed because " +
                                $"of its extension '{Path.GetExtension(e.File.Name)}'.";
                            break;
                        case ReadFileRejection.Type:
                            ReadFileListStatus = $"File '{e.File!.Name}' of type '{e.File.Type}' " +
                                "is not allowed.";
                            break;
                        case ReadFileRejection.Multiple:
                            ReadFileListStatus = $"Processing multiple files ({e.TotalFilesToRead}) " +
                                "is not allowed.";
                            break;
                        default:
                            break;
                    }
                    StateHasChanged();
                    break;
                case ReadFileEventType.Processed:
                    ReadFileListStatus = $"{e.FilesRead} of {e.TotalFilesToRead} file(s) processed.";
                    break;
                case ReadFileEventType.End:
                    FileReadStopped();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles various event types related to a single file reading operation.
        /// </summary>
        /// <param name="e">The event data holder.</param>
        protected virtual void HandleFileReaderProgressChanged(FileReaderProgressChangedEventArgs e)
        {
            var changeState = true;
            ReadProgress = e;
            if (IsDebug) Console.WriteLine($"FileReader event: {e.EventType} ({e.ProgressPercentage}%)");

            switch (e.EventType)
            {
                case ProgressChangedEventType.Start:
                    _currentFile = e.File;
                    ReadStatus = $"Preparing to read {_currentFile?.Name}...";

                    if (!_fileListReading)
                    {
                        StartStopWatch();
                        AbortRequested = false;
                        IsRunning = true;
                    }
                    break;
                case ProgressChangedEventType.Progress:
                    if (AbortRequested)
                    {
                        e.Cancel = true;
                        ReadStatus = "Aborting...";
                    }
                    else
                    {
                        ReadStatus = $"Reading {_currentFile?.Name}...";
                    }
                    break;
                case ProgressChangedEventType.Complete:
                    ReadStatus = "Done!";
                    break;
                case ProgressChangedEventType.Error:
                    ReadStatus = "An error occurred during a file read operation.";
                    break;
                case ProgressChangedEventType.Abort:
                    ReadStatus = "Operation aborted.";
                    break;
                case ProgressChangedEventType.End:
                    if (!_fileListReading)
                    {
                        FileReadStopped();
                        changeState = false;
                    }
                    break;
                default:
                    break;
            }

            if (IsDebug && ReadStatus != null) Console.WriteLine(ReadStatus);
            if (changeState) StateHasChanged();
        }

        /// <summary>
        /// Event fired each time a file list reading operation occurs.
        /// </summary>
        /// <param name="e">An object that holds event data related to the read operation.</param>
        /// <returns></returns>
        protected internal async Task<int> OnReadFileListAsync(ReadFileListEventArgs e)
        {
            HandleReadFileList(e);
            if (OnReadFileList.HasDelegate)
                await OnReadFileList.InvokeAsync(e);
            if (e.Cancel && e.Response == ReadFileRejection.None)
                e.Response = ReadFileRejection.Aborted;
            return (int)e.Response;
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

        /// <summary>
        /// Attaches a handler to the <see cref="EditContext.OnFieldChanged"/> event.
        /// </summary>
        protected virtual void AttachFieldChangedListener()
        {
            if (EditContext != null)
            {
                EditContext.OnFieldChanged += _fieldChangedHandler;
            }
        }

        /// <summary>
        /// Detaches a handler to the <see cref="EditContext.OnFieldChanged"/> event.
        /// </summary>
        protected virtual void DetachFieldChangedListener()
        {
            if (EditContext != null)
            {
                EditContext.OnFieldChanged -= _fieldChangedHandler;
            }
        }

        /// <inheritdoc/>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            _parametersSet = true;
            if (EditContext == null) SetEditContext();
        }

        #region helpers

        /// <summary>
        /// Updates a subset or all options that were used during script initialization.
        /// </summary>
        /// <param name="options">
        /// An object containing properties and values used to update 
        /// their respective matching properties in the script's option.
        /// </param>
        /// <returns></returns>
        protected async Task<bool> UpdateScriptOptionsAsync(object options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            var success = await JS!.InvokeAsync<bool>($"{BlazorFormManagerNS}.updateOptions", options);
            return success;
        }

        /// <summary>
        /// Throws <see cref="InvalidOperationException"/> if the BlazorFormManager.js
        /// script has not been initialized.
        /// </summary>
        protected virtual void EnsureScriptInitialized()
        {
            if (!_scriptInitialized)
                throw new InvalidOperationException($"{BlazorFormManagerNS}.js script has not been initialized.");
        }

        /// <summary>
        /// Detaches previous event handlers from the <see cref="EditContext"/>, and, 
        /// if the <see cref="Model"/> has its default value, invalidates it; otherwise,
        /// constructs an instance of the <see cref="Microsoft.AspNetCore.Components.Forms.EditContext"/>
        /// class then reattaches event handlers.
        /// </summary>
        protected virtual void SetEditContext()
        {
            if (!_parametersSet) return;

            DetachFieldChangedListener();

            if (EqualityComparer<object?>.Default.Equals(Model, default))
            {
                EditContext = null;
                return;
            }

            EditContext = new EditContext(Model);
            AttachFieldChangedListener();

            StateHasChanged();
        }

        /// <summary>
        /// Resets and/or starts the stop watch.
        /// </summary>
        private void StartStopWatch()
        {
            if (Stopwatch == null)
            {
                Stopwatch = new Stopwatch();
                _stopWatchTimer = new Timer(1000) { AutoReset = true };
                _stopWatchTimer.Elapsed += _stopWatchTimerElapsedHandler;
            }
            else
                Stopwatch.Reset();

            Stopwatch.Start();
        }

        internal IEnumerable<FormAttributeBase> GetAttributes(string propertyName, params Type[] attributeTypes)
        {
            return GetModel()?
                .GetType()
                .GetAttributes(propertyName, attributeTypes)
                ?? Enumerable.Empty<FormAttributeBase>();
        }

        internal FileCapableAttributeBase? GetFileCapableAttribute(string propertyName)
        {
            return GetFileCapableAttributes(propertyName, out _);
        }

        internal FileCapableAttributeBase? GetFileCapableAttributes(string propertyName, out ImagePreviewAttribute? imgAttr)
        {
            var attributes = GetAttributes(propertyName).ToArray();
            var attr = attributes.OfType<DragDropAttribute>().FirstOrDefault() ?? 
                attributes.OfType<FileCapableAttributeBase>().FirstOrDefault();
            
            imgAttr = attributes.OfType<ImagePreviewAttribute>().FirstOrDefault();
            return attr;
        }

        private void FileReadStopped()
        {
            _fileListReading = false;
            AbortRequested = false;
            IsRunning = false;
            ReadProgress = null;
            ReadStatus = null;
            ReadFileListProgress = null;
            ReadFileListStatus = null;
            _currentFile = null;
            Stopwatch?.Stop();
            StateHasChanged();
        }

        private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
        {
            Console.WriteLine($"###{nameof(HandleFieldChanged)}### event handler invoked!");
            HasChanges = true;

            if (EnableChangeTracking)
            {
                if (ValidateOnFieldChanged)
                    HasValidationErrors = true == EditContext?.Validate();
                
                EditContext?.NotifyFieldChanged(e.FieldIdentifier);
            }

            StateHasChanged();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases used resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release managed resources only;
        /// otherwise, false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DetachFieldChangedListener();

                    _thisObjRef?.Dispose();
                    
                    Stopwatch?.Stop();

                    if (_stopWatchTimer != null)
                    {
                        _stopWatchTimer.Enabled = false;
                        _stopWatchTimer.Elapsed -= _stopWatchTimerElapsedHandler;
                    }

                    if (_scriptInitialized)
                    {
                        try
                        {
                            _ = JS!.InvokeVoidAsync($"{BlazorFormManagerNS}.destroy", FormId);
                        }
                        catch
                        {
                        }
                    }
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
