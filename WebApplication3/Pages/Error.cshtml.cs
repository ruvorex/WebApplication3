using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace WebApplication3.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(int? code)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            ErrorCode = code.HasValue ? code.Value.ToString() : "Unknown";

            ErrorMessage = GetErrorMessageForStatusCode(code);
        }

        private string GetErrorMessageForStatusCode(int? code)
        {
            switch (code)
            {
                case 404:
                    return "The requested page was not found.";
                case 403:
                    return "Access is forbidden to the requested page.";
                case 500:
                    return "An internal server error occurred.";
                default:
                    return "An error occurred.";
            }
        }
    }
}
