namespace BlazorFormManager.Demo.Client.Models
{
    public class RegisterUserModelResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
        public bool SignedIn { get; set; }
    }
}
