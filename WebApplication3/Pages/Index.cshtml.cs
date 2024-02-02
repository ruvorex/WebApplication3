using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.Model;

namespace WebApplication3.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IndexModel> _logger;

        public ApplicationUser CurrentUser { get; set; }
        public string DecryptedNRIC { get; private set; }

        public IndexModel(UserManager<ApplicationUser> userManager, ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User is not logged in.");
                return RedirectToPage("/Login"); // Redirect to login page if not logged in
            }

            // Retrieve session GUID from cookie
            var sessionGuid = Request.Cookies["Session_Id"];
            if (sessionGuid == null || sessionGuid != user.CurrentSessionId)
            {
                // Session mismatch or expired, sign out the user
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                _logger.LogWarning("Session mismatch or expired. User signed out.");
                return RedirectToPage("/Login"); // Redirect to login page
            }

            CurrentUser = user;
            DecryptNRIC(user.NRIC);

            return Page();
        }

        private void DecryptNRIC(string encryptedNRIC)
        {
            try
            {
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("SecretKey");
                DecryptedNRIC = protector.Unprotect(encryptedNRIC);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error decrypting NRIC: {ex.Message}");
                DecryptedNRIC = $"Error decrypting NRIC: {ex.Message}";
            }
        }
    }

}