using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    [ValidateAntiForgeryToken]
    public class ChangePasswordModel : PageModel
    {
        [BindProperty]
        public ChangePassword CPModel { get; set; }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuditLogService _auditLogService;

        public ChangePasswordModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAuditLogService auditLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditLogService = auditLogService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            if (DateTime.UtcNow - user.LastPasswordChangeDate < TimeSpan.FromMinutes(10))
            {
                ModelState.AddModelError(string.Empty, "You can only change your password every 10 minutes.");
                return Page();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, CPModel.OldPassword, CPModel.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Update the LastPasswordChangeDate to the current time after successful password change
            user.LastPasswordChangeDate = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }


            await _auditLogService.LogAsync(user.Id, "Password changed");
            
            // Redirect the user to a confirmation page or back to the form with a success message.
            return RedirectToPage("/Login");
        }
    }
}

