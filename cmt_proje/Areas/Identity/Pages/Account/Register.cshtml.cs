using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using cmt_proje.Core.Constants;
using cmt_proje.Core.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace cmt_proje.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string? ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Full Name")]
            [RegularExpression(@"^\\s*\\S+(\\s+\\S+)+\\s*$", ErrorMessage = "Full name must contain at least two words.")]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Affiliation")]
            public string Affiliation { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Department")]
            public string Department { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Faculty")]
            public string Faculty { get; set; } = string.Empty;

            [Phone]
            [Display(Name = "WhatsApp Number")]
            public string? WhatsAppNumber { get; set; }

            [EmailAddress]
            [Display(Name = "Secondary Email")]
            public string? SecondaryEmail { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                
                user.FullName = Input.FullName;
                user.Affiliation = Input.Affiliation;
                user.Department = Input.Department;
                user.Faculty = Input.Faculty;
                user.PhoneNumber = Input.PhoneNumber;
                user.WhatsAppNumber = Input.WhatsAppNumber;
                user.SecondaryEmail = Input.SecondaryEmail;

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Yeni kullanıcıya otomatik olarak Author rolünü ata
                    var roleResult = await _userManager.AddToRoleAsync(user, AppRoles.Author);
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation("User {Email} assigned to Author role.", Input.Email);
                    }
                    else
                    {
                        // Rol atama başarısız olursa kullanıcıyı sil ve hata göster
                        _logger.LogError("Failed to assign Author role to user {Email}: {Errors}",
                            Input.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        
                        await _userManager.DeleteAsync(user);
                        
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, $"Role assignment failed: {error.Description}");
                        }
                        
                        return Page();
                    }

                    // Rol atama başarılı oldu, kullanıcıyı giriş yaptır
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User {Email} registered and signed in successfully.", Input.Email);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}

