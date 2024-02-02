using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace WebApplication3.ViewModels
{
    public class Register
    {
        [Required(ErrorMessage = "Please enter your first name")]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter your last name")]
        [DataType(DataType.Text)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please select your gender")]
        [DataType(DataType.Text)]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Please enter your NRIC")]
        [RegularExpression("^[STFG]\\d{7}[A-Z]$", ErrorMessage = "Invalid NRIC format")]
        [DataType(DataType.Text)]
        public string NRIC { get; set; }

        [Required(ErrorMessage = "Please enter a valid email address")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Enter at least a 12 characters password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{12,}$",
                    ErrorMessage = "Passwords must be at least 12 characters long and contain at least an uppercase letter, lower case letter, digit, and a symbol")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Enter at least a 12 characters password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{12,}$",
            ErrorMessage = "Passwords must be at least 12 characters long and contain at least an uppercase letter, lower case letter, digit, and a symbol")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please select a valid date of birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Please upload your resume in .docx or .pdf format")]
        [AllowedExtensions(new string[] { ".docx", ".pdf" }, ErrorMessage = "Invalid file format. Only .docx or .pdf allowed.")]
        [DataType(DataType.Upload)]
        public IFormFile Resume { get; set; }

        [Required(ErrorMessage = "Please provide information about yourself")]
        [DataType(DataType.MultilineText)]
        public string WhoAmI { get; set; }
    }

    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var fileName = Path.GetFileName(file.FileName);
                var fileExtension = Path.GetExtension(fileName);

                if (!_extensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                {
                    var allowedExtensions = string.Join(", ", _extensions);
                    return new ValidationResult($"Only {allowedExtensions} file types are allowed");
                }
            }

            return ValidationResult.Success;
        }
    }
}
