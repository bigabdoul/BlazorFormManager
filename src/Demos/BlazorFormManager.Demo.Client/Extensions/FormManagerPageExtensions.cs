using BlazorFormManager.Components;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace BlazorFormManager.Demo.Client.Extensions
{
    public static class FormManagerPageExtensions
    {
        #region fields

        private static readonly JsonSerializerOptions CaseInsensitiveJson = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        internal const string UNKNOWN_SUBMIT_ERROR =
            "We couldn't submit your data for an unknown reason. " +
            "If the problem persists, please contact your administrator";

        private const string SUCCESS_MESSAGE =
            "Congratulations, your account has been successfully updated!";

        #endregion

        /// <summary>
        /// Custom server response handler.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="result">
        /// A reference to the <see cref="FormManagerBase{TModel}.SubmitResult"/> property.
        /// </param>
        public static void ProcessCustomServerResponse(this FormManagerBase manager, FormManagerSubmitResult result)
        {
            // Don't update the state because it will be done by the
            // form manager once execution of this method is finished.
            var xhr = result.XHR;
            if (xhr.IsJsonResponse)
            {
                if (manager.IsDebug) Console.WriteLine($"Raw JSON result: {xhr.ResponseText}");

                try
                {
                    var postResult = JsonSerializer.Deserialize<PostFormHttpResult>(
                        xhr.ResponseText,
                        CaseInsensitiveJson
                    );

                    if (postResult.Success)
                        manager.SubmitResult = FormManagerSubmitResult.Success(result,
                            postResult.Message ?? SUCCESS_MESSAGE);
                    else if (true == xhr.ExtraProperties?.ContainsKey("error"))
                        manager.SubmitResult = FormManagerSubmitResult.Failed(result,
                            xhr.ExtraProperties["error"]?.ToString());
                    else
                        manager.SubmitResult = FormManagerSubmitResult.Failed(result,
                            postResult.GetErrorDetails() ?? UNKNOWN_SUBMIT_ERROR);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }
            else if (manager.IsDebug)
            {
                if (xhr.IsHtmlResponse)
                    Console.WriteLine($"HTML result: {xhr.ResponseText}");
                else
                    Console.WriteLine($"Raw result: {xhr.ResponseText}");
            }
        }
    }
}
