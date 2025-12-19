using System.Diagnostics;
using cmt_proje.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmt_proje.Infrastructure.Data;
using cmt_proje.Core.Constants;

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

            return View(activeConferences);
        }

        [Authorize]
        public IActionResult Home()
        {
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
