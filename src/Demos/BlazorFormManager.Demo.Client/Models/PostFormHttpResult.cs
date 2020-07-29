using System.Collections.Generic;

namespace BlazorFormManager.Demo.Client
{
    internal class PostFormHttpResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }

        // {"type":"https://tools.ietf.org/html/rfc7231#section-6.5.1","title":"One or more validation errors occurred.","status":400,"traceId":"|ff77f107-41102e0e88267362.","errors":{"Code":["The Code field is required."]}}
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }

        public string GetErrorDetails()
        {
            var sb = new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(Title)) sb.AppendLine($"Title: {Title}");

            if (!string.IsNullOrEmpty(Error)) sb.AppendLine(Error);
            else if (!string.IsNullOrEmpty(Message)) sb.AppendLine(Message);

            if (Status > 0) sb.AppendLine($"Status: {Status}");

            if (Errors != null && Errors.Count > 0)
            {
                sb.AppendLine("Errors:");
                foreach (var kvp in Errors)
                {
                    sb.AppendLine($"{kvp.Key} :");
                    foreach (var err in kvp.Value)
                    {
                        sb.AppendLine(err);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
