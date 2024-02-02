using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.Model;

namespace WebApplication3.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IAuditLogService auditLogService;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IAuditLogService auditLogService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.auditLogService = auditLogService;
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                HttpContext.Session.Clear();
                user.CurrentSessionId = null;
                await auditLogService.LogAsync(user.Id, "User logged out");
                await userManager.UpdateAsync(user);
            }

            await signInManager.SignOutAsync();
            return RedirectToPage("Login");
        }

        public IActionResult OnPostDontLogoutAsync()
        {
            return RedirectToPage("Index");
        }
    }
}
