using System.Linq;
using System.Threading.Tasks;
using cmt_proje.Core.Entities;
using cmt_proje.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cmt_proje.Controllers
{
    [AllowAnonymous]
    public class AboutController : Controller
    {
        private readonly ConferenceDbContext _context;

        public AboutController(ConferenceDbContext context)
        {
            _context = context;
        }

        private async Task SetConferenceContext(int? conferenceId)
        {
            if (conferenceId.HasValue)
            {
                var acronym = await _context.Conferences
                    .AsNoTracking()
                    .Where(c => c.Id == conferenceId.Value)
                    .Select(c => c.Acronym)
                    .FirstOrDefaultAsync();

                if (acronym != null)
                {
                    ViewBag.ActiveConferenceId = conferenceId.Value;
                    ViewBag.ActiveConferenceAcronym = acronym;
                }
            }
        }

        // GET: /About
        public IActionResult Index(int? conferenceId = null)
        {
            return RedirectToAction("PastEvents", new { conferenceId });
        }

        // GET: /About/PastEvents
        public async Task<IActionResult> PastEvents(int? conferenceId = null)
        {
            await SetConferenceContext(conferenceId);

            var content = await _context.AboutContents
                .AsNoTracking()
                .Where(a => a.PageKey == "PastEvents")
                .FirstOrDefaultAsync();

            if (content == null)
            {
                // Veritabanında kayıt yoksa, default içerik göster
                content = new AboutContent
                {
                    PageKey = "PastEvents",
                    Title = "Past Events",
                    Content = "Bu sayfanın içeriği henüz eklenmemiştir.",
                    LastUpdated = System.DateTime.UtcNow
                };
            }

            return View(content);
        }

        // GET: /About/AboutOrganizer
        public async Task<IActionResult> AboutOrganizer(int? conferenceId = null)
        {
            await SetConferenceContext(conferenceId);

            // Veritabanından en güncel veriyi oku
            var content = await _context.AboutContents
                .AsNoTracking()
                .Where(a => a.PageKey == "AboutOrganizer")
                .OrderByDescending(a => a.LastUpdated)
                .FirstOrDefaultAsync();

            if (content == null)
            {
                // Veritabanında kayıt yoksa, default içerik göster
                content = new AboutContent
                {
                    PageKey = "AboutOrganizer",
                    Title = "About Organizer",
                    Content = "Bu sayfanın içeriği henüz eklenmemiştir.",
                    LastUpdated = System.DateTime.UtcNow
                };
            }

            return View(content);
        }

        // GET: /About/Testimonials
        public async Task<IActionResult> Testimonials(int? conferenceId = null)
        {
            await SetConferenceContext(conferenceId);

            var content = await _context.AboutContents
                .AsNoTracking()
                .Where(a => a.PageKey == "Testimonials")
                .FirstOrDefaultAsync();

            if (content == null)
            {
                // Veritabanında kayıt yoksa, default içerik göster
                content = new AboutContent
                {
                    PageKey = "Testimonials",
                    Title = "Testimonials / Reviews",
                    Content = "Bu sayfanın içeriği henüz eklenmemiştir.",
                    LastUpdated = System.DateTime.UtcNow
                };
            }

            return View(content);
        }
    }
}

