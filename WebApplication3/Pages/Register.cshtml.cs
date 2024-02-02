using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.ViewModels;
using WebApplication3.Model;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Encodings.Web;
using Newtonsoft.Json;
using System;

namespace WebApplication3.Pages
{
    [ValidateAntiForgeryToken]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IServiceProvider serviceProvider; 

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment env, ILogger<RegisterModel> logger, IDataProtectionProvider dataProtectionProvider, IServiceProvider serviceProvider)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            webHostEnvironment = env;
            _logger = logger;
            _dataProtectionProvider = dataProtectionProvider;
            this.serviceProvider = serviceProvider; 
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("This is a log message");
            if (ModelState.IsValid)
            {
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

                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("SecretKey");

                var existingUser = await userManager.FindByEmailAsync(RModel.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Email address is already in use.");
                    return Page();
                }

                var user = new ApplicationUser()
                {
                    UserName = RModel.Email,
                    Email = RModel.Email,
                    FirstName = SanitizeInput(RModel.FirstName),
                    LastName = SanitizeInput(RModel.LastName),
                    Gender = SanitizeInput(RModel.Gender),
                    NRIC = protector.Protect(RModel.NRIC),
                    DateOfBirth = RModel.DateOfBirth,
                    WhoAmI = SanitizeInput(RModel.WhoAmI),
                    LastPasswordChangeDate = DateTime.UtcNow 
                };

                if (RModel.Resume != null && RModel.Resume.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + "_" + RModel.Resume.FileName;
                    var filePath = Path.Combine(webHostEnvironment.ContentRootPath, @"wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await RModel.Resume.CopyToAsync(stream);
                    }

                    // Update user's resume information
                    user.ResumePath = filePath;
                }

                var result = await userManager.CreateAsync(user, RModel.Password);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToPage("Index");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return Page();
        }

        // Function to sanitize user input using AntiXSS
        private string SanitizeInput(string input)
        {
            // Encode the input to prevent XSS attacks
            var sanitizedInput = HtmlEncoder.Default.Encode(input);

            return sanitizedInput;
        }
    }
}
