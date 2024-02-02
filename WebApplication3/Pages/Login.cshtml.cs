using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.ViewModels;
using WebApplication3.Model;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace WebApplication3.Pages
{
    [ValidateAntiForgeryToken]
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IAuditLogService _auditLogService;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IAuditLogService auditLogService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            _auditLogService = auditLogService;
        }


        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {

                var user = await signInManager.UserManager.FindByEmailAsync(LModel.Email);

                if (user != null)
                {
					// Verify reCAPTCHA
					var recaptchaResponse = Request.Form["g-recaptcha-response"];
					var recaptchaSecretKey = "6LfG2WMpAAAAAKKLixCzMy64STCalxH3_rCU8nCv";

					var httpClient = new HttpClient();
					var response = await httpClient.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={recaptchaSecretKey}&response={recaptchaResponse}");


					var recaptchaResult = JsonConvert.DeserializeObject<RecaptchaResponse>(response);

					if (!recaptchaResult.Success)
					{
						ModelState.AddModelError("", "reCAPTCHA validation failed. Please try again.");

						if (recaptchaResult.ErrorCodes != null)
						{
							foreach (var errorCode in recaptchaResult.ErrorCodes)
							{
								ModelState.AddModelError("", $"reCAPTCHA Error: {errorCode}");
							}
						}

						return Page();
					}

                    var identityResult = await signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, LModel.RememberMe, lockoutOnFailure: true);

                    if (identityResult.Succeeded)
                    {
                        string guid = Guid.NewGuid().ToString();
                        var cookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTime.UtcNow.AddHours(1) // Set your desired expiration time
                        };
                        Response.Cookies.Append("Session_Id", guid, cookieOptions);

                        user.CurrentSessionId = guid;
                        await userManager.UpdateAsync(user);

                        await _auditLogService.LogAsync(user.Id, "User logged in");

                        return RedirectToPage("Index");
                    }
                    else if (identityResult.IsLockedOut)
                    {
                        ModelState.AddModelError("", "Account locked out. Please try again later.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Username or Password incorrect");
                    }
                }
            }
            return Page();
        }

    }

	public class RecaptchaResponse
	{
		public bool Success { get; set; }
		public List<string> ErrorCodes { get; set; }
	}
}
