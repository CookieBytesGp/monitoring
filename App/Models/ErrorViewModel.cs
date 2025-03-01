namespace App.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // Add this property for the error message
        public string ErrorMessage { get; set; }
    }
}
