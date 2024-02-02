using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.ViewModels;
using WebApplication3.Model;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Encodings.Web;

namespace WebApplication3.Pages
{
	public class RegisterModel : PageModel
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly IWebHostEnvironment webHostEnvironment;
		private readonly ILogger<RegisterModel> _logger;
        private readonly IDataProtectionProvider _dataProtectionProvider; 

        [BindProperty]
		public Register RModel { get; set; }

		public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment env, ILogger<RegisterModel> logger, IDataProtectionProvider dataProtectionProvider)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			webHostEnvironment = env;
			_logger = logger;
            _dataProtectionProvider = dataProtectionProvider;
        }
		public void OnGet()
		{
		}

		public async Task<IActionResult> OnPostAsync()
		{
			_logger.LogInformation("This is a log message");
			if (ModelState.IsValid)
			{

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
                    WhoAmI = SanitizeInput(RModel.WhoAmI)
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

