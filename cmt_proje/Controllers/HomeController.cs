using System.Diagnostics;
using System.Text.RegularExpressions;
using cmt_proje.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmt_proje.Infrastructure.Data;
using cmt_proje.Core.Constants;
using cmt_proje.Core.Entities;

namespace cmt_proje.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ConferenceDbContext _context;

        public HomeController(ILogger<HomeController> logger, ConferenceDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Helper method to generate slug from acronym
        private string GenerateSlug(string? acronym)
        {
            if (string.IsNullOrWhiteSpace(acronym))
                return string.Empty;
            
            var slug = acronym.ToLowerInvariant()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "");
            
            slug = Regex.Replace(slug, @"[^a-z0-9]", "");
            
            return slug;
        }

        // Helper method to ensure unique slug
        private async Task<string> EnsureUniqueSlug(string baseSlug, int excludeId)
        {
            var slug = baseSlug;
            var counter = 1;
            
            while (await _context.Conferences.AnyAsync(c => c.Slug == slug && c.Id != excludeId))
            {
                slug = $"{baseSlug}{counter}";
                counter++;
            }
            
            return slug;
        }

        public async Task<IActionResult> Index()
        {
            // Eğer kullanıcı giriş yapmışsa Home'a yönlendir
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Home");
            }

            // Public landing page için aktif konferansları getir
            var activeConferences = await _context.Conferences
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .Take(3)
                .ToListAsync();
            
            // Ensure all conferences have slugs
            foreach (var conf in activeConferences)
            {
                if (string.IsNullOrWhiteSpace(conf.Slug))
                {
                    var slug = GenerateSlug(conf.Acronym);
                    conf.Slug = await EnsureUniqueSlug(slug, conf.Id);
                    var confToUpdate = await _context.Conferences.FindAsync(conf.Id);
                    if (confToUpdate != null)
                    {
                        confToUpdate.Slug = conf.Slug;
                    }
                }
            }
            await _context.SaveChangesAsync();

            // Hero content'i veritabanından getir
            var heroContent = await _context.HomeHeroContents
                .AsNoTracking()
                .OrderByDescending(h => h.LastUpdated)
                .FirstOrDefaultAsync();

            // ViewBag'e hero content'i ekle
            ViewBag.HeroTitle = heroContent?.HeroTitle ?? "The Meeting Point<br />of Scientific Advancements";
            ViewBag.HeroSubtitle = heroContent?.HeroSubtitle ?? "Explore world-class academic conferences hosted by Sakarya University.";

            return View(activeConferences);
        }

        [Authorize]
        public async Task<IActionResult> Home()
        {
            // Hero content'i veritabanından getir
            var heroContent = await _context.HomeHeroContents
                .AsNoTracking()
                .OrderByDescending(h => h.LastUpdated)
                .FirstOrDefaultAsync();

            // ViewBag'e hero content'i ekle
            ViewBag.HeroTitle = heroContent?.HeroTitle ?? "The Meeting Point<br />of Scientific Advancements";
            ViewBag.HeroSubtitle = heroContent?.HeroSubtitle ?? "Explore world-class academic conferences hosted by Sakarya University.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Tracks sayfasına yönlendir - Konferans seçim sayfasına git
        /// </summary>
        public IActionResult GoToTracks()
        {
            return RedirectToAction("SelectConference", "Tracks");
        }

        /// <summary>
        /// Decisions sayfasına yönlendir - Konferans seçim sayfasına git (Chair için)
        /// </summary>
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = AppRoles.Chair)]
        public IActionResult GoToDecisions()
        {
            return RedirectToAction("SelectConferenceForDecisions");
        }

        /// <summary>
        /// Decisions için Konferans Seçim Sayfası
        /// </summary>
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> SelectConferenceForDecisions()
        {
            var conferences = await _context.Conferences
                .Include(c => c.CreatedByUser)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(conferences);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
