using System.Diagnostics;
using cmt_proje.Models;
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

        public IActionResult Index()
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
        /// Decisions sayfasına yönlendir - İlk aktif konferansın submissions sayfasına git (Chair için)
        /// </summary>
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> GoToDecisions()
        {
            var firstActiveConference = await _context.Conferences
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            if (firstActiveConference != null)
            {
                return RedirectToAction("Index", "Submissions", new { conferenceId = firstActiveConference.Id });
            }

            // Aktif konferans yoksa, tüm konferansları göster
            return RedirectToAction("Index", "Conferences");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
