using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using cmt_proje.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace cmt_proje.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly cmt_proje.Infrastructure.Data.ConferenceDbContext _dbContext;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            cmt_proje.Infrastructure.Data.ConferenceDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Full Name")]
            [StringLength(100, ErrorMessage = "The {0} must be at most {1} characters long.")]
            public string? FullName { get; set; }

            [Display(Name = "Affiliation")]
            [StringLength(200, ErrorMessage = "The {0} must be at most {1} characters long.")]
            public string? Affiliation { get; set; }

            [Display(Name = "Department")]
            [StringLength(100, ErrorMessage = "The {0} must be at most {1} characters long.")]
            public string? Department { get; set; }

            [Display(Name = "Faculty")]
            [StringLength(100, ErrorMessage = "The {0} must be at most {1} characters long.")]
            public string? Faculty { get; set; }
        }
        
        // Helper to load conference context for View info
        private async Task LoadConferenceContext(int? conferenceId)
        {
             if (conferenceId.HasValue)
            {
                 var confAcronym = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                    System.Linq.Queryable.Select(
                        System.Linq.Queryable.Where(
                            Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsNoTracking(_dbContext.Conferences), 
                            c => c.Id == conferenceId.Value),
                        c => c.Acronym)
                );

                if (confAcronym != null)
                {
                    ViewData["ActiveConferenceId"] = conferenceId.Value;
                    ViewData["ActiveConferenceAcronym"] = confAcronym;
                }
            }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            Username = userName ?? string.Empty;
            Email = email;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FullName = user.FullName,
                Affiliation = user.Affiliation,
                Department = user.Department,
                Faculty = user.Faculty
            };
        }

        public async Task<IActionResult> OnGetAsync(int? conferenceId = null)
        {
            await LoadConferenceContext(conferenceId);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? conferenceId = null)
        {
             await LoadConferenceContext(conferenceId); // Maintain context on valid/invalid post

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage(new { conferenceId });
                }
            }

            // Özel alanları güncelle
            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
            }

            if (Input.Affiliation != user.Affiliation)
            {
                user.Affiliation = Input.Affiliation;
            }

            if (Input.Department != user.Department)
            {
                user.Department = Input.Department;
            }

            if (Input.Faculty != user.Faculty)
            {
                user.Faculty = Input.Faculty;
            }

            // Değişiklikleri kaydet
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage(new { conferenceId });
        }
    }
}
