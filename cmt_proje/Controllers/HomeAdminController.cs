using System;
using System.Linq;
using System.Threading.Tasks;
using cmt_proje.Core.Constants;
using cmt_proje.Core.Entities;
using cmt_proje.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cmt_proje.Controllers
{
    [Authorize(Roles = AppRoles.Chair)]
    public class HomeAdminController : Controller
    {
        private readonly ConferenceDbContext _context;

        public HomeAdminController(ConferenceDbContext context)
        {
            _context = context;
        }

        // GET: /HomeAdmin/Edit
        public async Task<IActionResult> Edit()
        {
            var heroContent = await _context.HomeHeroContents
                .OrderByDescending(h => h.LastUpdated)
                .FirstOrDefaultAsync();

            if (heroContent == null)
            {
                // Eğer kayıt yoksa, yeni oluştur
                heroContent = new HomeHeroContent
                {
                    HeroTitle = "The Meeting Point<br />of Scientific Advancements",
                    HeroSubtitle = "Explore world-class academic conferences hosted by Sakarya University.",
                    LastUpdated = DateTime.UtcNow
                };
                _context.HomeHeroContents.Add(heroContent);
                await _context.SaveChangesAsync();
            }

            return View(heroContent);
        }

        // POST: /HomeAdmin/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string HeroTitle, string HeroSubtitle)
        {
            try
            {
                var heroTitleValue = Request.Form["HeroTitle"].ToString();
                var heroSubtitleValue = Request.Form["HeroSubtitle"].ToString();

                // Entity'yi TRACKED olarak bul
                var existingContent = await _context.HomeHeroContents
                    .FirstOrDefaultAsync(h => h.Id == Id);

                if (existingContent == null)
                {
                    TempData["ErrorMessage"] = "Hero content not found.";
                    return RedirectToAction(nameof(Edit));
                }

                // Tracked entity'nin property'lerini direkt güncelle
                existingContent.HeroTitle = !string.IsNullOrEmpty(heroTitleValue) ? heroTitleValue : (HeroTitle ?? string.Empty);
                existingContent.HeroSubtitle = !string.IsNullOrEmpty(heroSubtitleValue) ? heroSubtitleValue : (HeroSubtitle ?? string.Empty);
                existingContent.LastUpdated = DateTime.UtcNow;

                // Değişiklikleri kaydet
                var savedCount = await _context.SaveChangesAsync();

                if (savedCount > 0)
                {
                    TempData["SuccessMessage"] = "Hero content has been updated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No changes were saved. Please try again.";
                }

                return RedirectToAction(nameof(Edit));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HomeHeroContentExists(Id))
                {
                    TempData["ErrorMessage"] = "Hero content not found. It may have been deleted.";
                    return RedirectToAction(nameof(Edit));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                var errorDetails = $"An error occurred while updating the content: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorDetails += $" Inner: {ex.InnerException.Message}";
                }
                TempData["ErrorMessage"] = errorDetails;
                return RedirectToAction(nameof(Edit));
            }
        }

        private bool HomeHeroContentExists(int id)
        {
            return _context.HomeHeroContents.Any(e => e.Id == id);
        }
    }
}

