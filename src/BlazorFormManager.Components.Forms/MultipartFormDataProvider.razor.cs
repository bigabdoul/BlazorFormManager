using Carfamsoft.Model2View.Shared.Collections;
using Carfamsoft.Model2View.Shared.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicForm.Abstractions.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace BlazorFormManager.Components.Forms;

[INotifyPropertyChanged]
public partial class MultipartFormDataProviderBase : ComponentBase
{

    [ObservableProperty]
    private Exception? _error;

    [ObservableProperty]
    private bool _hasEntity;

    [ObservableProperty]
    private bool _newRequested;

    [ObservableProperty]
    private bool _savedSuccessfully;

    [ObservableProperty]
    private IEnumerable<UploadResult>? _uploadResults;

    [ObservableProperty]
    private object? _entityPrimaryKey;

    private object? currentModel;
    private const string MULTIPART_FORM_DATA = "multipart/form-data";

    [Inject] private HttpClient Http { get; set; } = null!;

    [Parameter] public IDictionary<string, object?>? Model { get; set; }
    [Parameter] public object? PocoModel { get; set; }

    /// <summary>
    /// The maximum allowed file size in bytes.
    /// </summary>
    [Parameter] public long MaxFileSize { get; set; }
    [Parameter] public string? RequestUrl { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method to use when submitting the form.
    /// Only POST and PUT methods are currently supported.
    /// </summary>
    [Parameter] public string? FormMethod { get; set; }

    /// <summary>
    /// Gets or sets the form's encoding type.
    /// </summary>
    [Parameter] public string? FormEncoding { get; set; }

    /// <summary>
    /// Gets or sets an alternative <see cref="System.Net.Http.HttpClient"/> to use.
    /// </summary>
    [Parameter] public HttpClient? HttpClient { get; set; }
    [Parameter] public ILogger? Logger { get; set; }
    [Parameter] public bool ShowError { get; set; }
    [Parameter] public bool ParseResponse { get; set; } = true;
    [Parameter] public EventCallback<FormDataProviderEventArgs> OnDataSending { get; set; }
    [Parameter] public EventCallback<IDataFormUpdateResult?> OnDataSent { get; set; }
    [Parameter] public EventCallback<string?> OnSuccess { get; set; }
    [Parameter] public EventCallback<Exception> OnError { get; set; }
    [CascadingParameter] protected FormManagerBase? Form { get; set; }

    public virtual bool HasChanges { get; protected set; }

    /// <summary>
    /// Determines if the form contains any file to upload.
    /// </summary>
    public bool HasFiles => true == Model?.Values.Any(v => v is IEnumerable<IBrowserFile>);

    /// <summary>
    /// Suffix appended to the a file property name to avoid replacing the property's
    /// value (on the 'src' attribute of an 'img' tag) with the browser files list.
    /// If not properly addressed, this behavior causes a bug on the <see cref="InputFile" />
    /// component when trying to persist the changes in an <see cref="IDataFormViewModel" />.
    /// Therefore, this suffix should be removed from the property name when submitting
    /// the browser files' contents.
    /// </summary>
    const string IMG_SUFFIX = "_BROWSER_FILES_";

    protected override void OnInitialized()
    {
        if (Form?.Model != null)
        {
            PocoModel ??= Form.Model;
            FormMethod ??= Form.FormMethod ?? "POST";

            var originalOnFieldChanged = Form.OnFieldChanged;
            var originalOnBeforeSubmit = Form.OnBeforeSubmit;

            Form.SetOnFieldChanged(EventCallback.Factory.Create<FormFieldChangedEventArgs>(Form, async e =>
            {
                if (e.Value is IEnumerable<IBrowserFile> files)
                {
                    SetValue($"{e.Field.FieldName}{IMG_SUFFIX}", files);
                }
                else
                {
                    SetValue(e.Field.FieldName, e.Value);
                }
                if (originalOnFieldChanged.HasDelegate)
                    await originalOnFieldChanged.InvokeAsync(e);
            }));

            Form.SetOnBeforeSubmit(EventCallback.Factory.Create<CancelEventArgs>(Form, async e =>
            {
                if (originalOnBeforeSubmit.HasDelegate)
                    await originalOnBeforeSubmit.InvokeAsync(e);

                if (!e.Cancel)
                    await HandleBeforeFormSubmit(e);
                else if (Logger?.IsEnabled(LogLevel.Warning) == true)
                    Logger?.LogWarning("Form submission has been cancelled.");
            }));
        }
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        if (!Equals(currentModel, PocoModel))
        {
            if (PocoModel is not null)
            {
                Model = new ObjectDictionary(PocoModel);
            }
            else
            {
                Model?.Clear();
            }
            currentModel = PocoModel;
        }
        base.OnParametersSet();
    }

    public virtual void SetValue(string propertyName, object? value)
    {
        Model ??= new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (Model.TryGetValue(propertyName, out var result))
        {
            if (!Equals(result, value))
            {
                Model[propertyName] = value;
                HasChanges = true;
                OnPropertyChanged(propertyName);
            }
        }
        else
        {
            Model[propertyName] = value;
            HasChanges = true;
            OnPropertyChanged(propertyName);
        }
    }

    public virtual async Task<IDataFormUpdateResult?> SubmitFormDataAsync(string url)
    {
        var client = HttpClient ?? Http;
        var json = GetEncoding().Contains("json", StringComparison.OrdinalIgnoreCase);

        if (json && !HasFiles)
        {
            UploadResults = null;
            var response = await
            (
                IsPost ? client.PostAsJsonAsync(url, Model) :
                IsPut ? client.PutAsJsonAsync(url, Model) : throw HttpMethodNotSupported()
            );
            return await ParseResponseAsync(response, null);
        }
        else if (json && HasFiles)
            SetAndLogError(new InvalidOperationException("Cannot submit the form data as JSON when it contains one or more files."));
        else
        {
            // merge the model's entries into a new dictionary
            // suitable for posting form URL-encoded content
            var formContent = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase).ConvertMerge(Model!,
                converter: (key, value) => $"{value}",

                // we'll treat a collection of IBrowserFile objects in a special way
                skipIf: (key, value) => value is null || value is IEnumerable<IBrowserFile>);

            return await SubmitFormDataAsync(url, formContent);
        }
        return null;
    }

    public virtual async Task<IDataFormUpdateResult?> SubmitFormDataAsync(string url, IEnumerable<KeyValuePair<string, string>> formContent)
    {
        UploadResults = null;
        var client = HttpClient ?? Http;
        var httpContent = GetHttpContent(formContent);
        var response = await
        (
            IsPost ? client.PostAsync(url, httpContent) :
            IsPut ? client.PutAsync(url, httpContent) : throw HttpMethodNotSupported()
        );
        return await ParseResponseAsync(response, httpContent);
    }

    private async Task<IDataFormUpdateResult?> ParseResponseAsync(HttpResponseMessage response, HttpContent? httpContent)
    {
        if (!ParseResponse)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                SetError(content, throwErr: true);
            }
            else if (OnSuccess.HasDelegate)
            {
                Logger?.LogInformation("Form data successfully submitted.");
                await OnSuccess.InvokeAsync(content);
            }
            return null;
        }

        if (response.IsSuccessStatusCode)
        {
            if (OnSuccess.HasDelegate)
                await OnSuccess.InvokeAsync();

            DataFormUpdateResult? result;
            try
            {
                result = await response.Content.ReadFromJsonAsync<DataFormUpdateResult>();

                if (result is not null)
                {
                    result.Entity.Merge(Model); // merge the dictionary into the model

                    if (result.ScopeIdentityValue != null)
                    {
                        HasEntity = true;
                        EntityPrimaryKey = result.ScopeIdentityValue;
                    }

                    if (httpContent is MultipartFormDataContent)
                    {
                        UploadResults = result.Uploads;
                        RemoveUploadedBrowserFiles();

                        var unsavedFiles = result.Uploads?.Where(f => f.Uploaded == false).ToArray();

                        if (unsavedFiles?.Length > 0)
                        {
                            SetAndLogError(new Exception
                            (
                                //"Attention ! Les fichiers téléversés suivants n'ont pas été sauvegardés : "
                                "Warning! The following uploaded files have not been saved: "
                                + string.Join(", ", unsavedFiles.Select(f => f.FileName))
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = new();
                SetAndLogError(ex);
            }

            NewRequested = false;
            HasChanges = false;
            SavedSuccessfully = true;

            return result;
        }
        else
        {
            string? content = null;
            try
            {
                content = await response.Content.ReadAsStringAsync();
                if (content.IsNotBlank())
                {
                    var responseError = JsonSerializer.Deserialize<HttpResponseErrorModel>(content);

                    if (responseError?.Error != null)
                        SetAndLogError(new HttpRequestException(responseError.Error.ToString(), inner: null, response.StatusCode));
                    else
                        SetAndLogError(new HttpRequestException(response.ReasonPhrase, inner: null, response.StatusCode));
                }
                else throw new HttpRequestException();
            }
            catch
            {
                SetError(content);
            }
            return null;
        }

        void SetError(string? content, bool throwErr = false)
        {
            var error = new HttpRequestException($"HTTP Error {(int)response.StatusCode}. " +
                    $"Reason Phrase: {response.ReasonPhrase}. " +
                    $"Response Content: {(content.IsNotBlank() ? content : "[No Content]")}",
                    inner: null, response.StatusCode);
            SetAndLogError(error);
            if (throwErr) throw error;
        }
    }

    /// <summary>
    /// Returns either a <see cref="FormUrlEncodedContent"/> object or - if the form
    /// contains browser files or the encoding is multipart/form-data - a 
    /// <see cref="MultipartFormDataContent"/> object to post.
    /// </summary>
    /// <param name="formContent">The form data content to include.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    /// The form encoding must be multipart/form-data when uploading files.
    /// </exception>
    protected virtual HttpContent GetHttpContent(IEnumerable<KeyValuePair<string, string>> formContent)
    {
        var isMultipart = IsMultipartFormData();
        
        if (!isMultipart && HasFiles)
            throw new InvalidOperationException($"The form encoding must be multipart/form-data when uploading files.");

        return isMultipart ? GetFormDataContent(formContent) : new FormUrlEncodedContent(formContent);
    }

    /// <summary>
    /// Returns an initialized <see cref="MultipartFormDataContent"/> object
    /// containing the specified form data content and any user-selected browser files.
    /// </summary>
    /// <param name="formContent">The form data content to include.</param>
    /// <returns></returns>
    protected virtual MultipartFormDataContent GetFormDataContent(IEnumerable<KeyValuePair<string, string>>? formContent = null)
    {
        MultipartFormDataContent content = new();

        if (formContent is not null)
        {
            foreach (var (key, value) in formContent)
            {
                content.Add(new StringContent(value), key);
            }
        }

        var dic = Model;

        if (dic is not null)
        {
            foreach (var (key, value) in dic)
            {
                if (value is IEnumerable<IBrowserFile> files)
                {
                    var propertyName = key;

                    // remove the suffix, if any, to retrieve the property name
                    if (propertyName.EndsWith(IMG_SUFFIX))
                        propertyName = propertyName[..^IMG_SUFFIX.Length];

                    foreach (var file in files)
                    {
                        var fileContent = new StreamContent(file.OpenReadStream(MaxFileSize));
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        content.Add(content: fileContent, name: $"\"{propertyName}\"", fileName: file.Name);
                    }
                }
            }
        }

        return content;
    }

    /// <summary>
    /// Removes the successfully uploaded browser files from the model dictionary
    /// to avoid resubmitting them on the next change save operation.
    /// </summary>
    protected virtual void RemoveUploadedBrowserFiles() => RemoveBrowserFiles(UploadResults?.Where(f => f.Uploaded));

    /// <summary>
    /// Removes the specified browser files from the model dictionary after a successful
    /// upload operation to avoid resubmitting them on the next change save operation.
    /// </summary>
    /// <param name="files">A collection of browser files to remove from the model dictionary.</param>
    protected virtual void RemoveBrowserFiles(IEnumerable<IUploadResult>? files)
    {
        if (files is null) return;
        var dic = Model;
        if (dic is null) return;

        // get all browser files in the dictionary and remove them;
        // first, collect the keys
        var keys = new List<string>();

        foreach (var (key, value) in dic.Where(v => v.Value is IEnumerable<IBrowserFile>))
        {
            var browserFiles = (IEnumerable<IBrowserFile>)value!;
            foreach (var file in files)
            {
                foreach (var bf in browserFiles)
                {
                    if (string.Equals(bf.Name, file.FileName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        keys.Add(key);
                        break;
                    }
                }
            }
        }

        // then remove the matching entries
        foreach (var key in keys)
            dic.Remove(key);
    }

    private async Task HandleBeforeFormSubmit(CancelEventArgs e)
    {
        e.Cancel = true;
        var dic = Model;
        if (dic is null)
        {
            SetAndLogError(new InvalidOperationException("The Model is null."));
            return;
        }
        else
        {
            Error = null;
        }

        try
        {
            if (OnDataSending.HasDelegate)
            {
                var ev = new FormDataProviderEventArgs(dic);
                await OnDataSending.InvokeAsync(ev);

                if (ev.Cancel)
                {
                    Error = ev.Error;
                    return;
                }
            }

            var url = RequestUrl.IsNotBlank() ? RequestUrl : Form?.FormAction;

            if (url.IsBlank())
            {
                SetAndLogError(new InvalidOperationException("The RequestUrl and FormManagerBase.FormAction values cannot be empty."));
                return;
            }

            var updateResult = await SubmitFormDataAsync(url!);
            
            if (OnDataSent.HasDelegate)
                await OnDataSent.InvokeAsync(updateResult);
        }
        catch (Exception ex)
        {
            SetAndLogError(ex);
            if (OnDataSent.HasDelegate)
                await OnDataSent.InvokeAsync(null);
        }
    }

    private void SetAndLogError(Exception error)
    {
        Error = error;
        LogError();
    }

    private void LogError()
    {
        var err = Error;
        var logger = Logger;
        if (err != null)
        {
            if (OnError.HasDelegate) 
                OnError.InvokeAsync(err);
            if (logger?.IsEnabled(LogLevel.Error) == true)
                logger.LogError("{ErrorMessage}", err);
        }
    }

    string Method => FormMethod ?? "POST";
    protected bool IsPost => string.Equals(Method, "POST", StringComparison.OrdinalIgnoreCase);
    protected bool IsPut => string.Equals(Method, "PUT", StringComparison.OrdinalIgnoreCase);
    protected string GetEncoding() => FormEncoding ?? Form?.EncodingType ?? MULTIPART_FORM_DATA;
    protected bool IsMultipartFormData() => GetEncoding().EqualsIgnoreCase(MULTIPART_FORM_DATA);

    private static NotSupportedException HttpMethodNotSupported()
    {
        return new NotSupportedException("Invalid HTTP method: Only POST and PUT methods are supported.");
    }

    class HttpResponseErrorModel
    {
        // {"error":{"number":-2146233088,"message":"Exception of type 'ValueProviderNotFoundException' was thrown."}}
        public HttpResponseError Error { get; set; } = null!;
    }

    class HttpResponseError
    {
        public int Number { get; set; }
        public string Message { get; set; } = null!;

        public override string ToString()
        {
            return $"Error 0x{Number:x} : {Message}";
        }
    }
}