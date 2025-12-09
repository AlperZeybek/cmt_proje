using System;
using System.Linq;
using System.Threading.Tasks;
using cmt_proje.Core.Constants;
using cmt_proje.Core.Entities;
using cmt_proje.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace cmt_proje.Controllers
{
    [Authorize(Roles = AppRoles.Chair)]
    public class AboutAdminController : Controller
    {
        private readonly ConferenceDbContext _context;

        public AboutAdminController(ConferenceDbContext context)
        {
            _context = context;
        }

        // GET: /AboutAdmin/Index
        public async Task<IActionResult> Index()
        {
            var contents = await _context.AboutContents
                .OrderBy(a => a.PageKey)
                .ToListAsync();

            // Eğer hiç kayıt yoksa, varsayılan kayıtları oluştur
            if (!contents.Any())
            {
                var defaultContents = new[]
                {
                    new AboutContent { PageKey = "PastEvents", Title = "Past Events", Content = "Bu sayfanın içeriği henüz eklenmemiştir.", LastUpdated = DateTime.UtcNow },
                    new AboutContent { PageKey = "AboutOrganizer", Title = "About Organizer", Content = "Bu sayfanın içeriği henüz eklenmemiştir.", LastUpdated = DateTime.UtcNow },
                    new AboutContent { PageKey = "Testimonials", Title = "Testimonials / Reviews", Content = "Bu sayfanın içeriği henüz eklenmemiştir.", LastUpdated = DateTime.UtcNow }
                };

                _context.AboutContents.AddRange(defaultContents);
                await _context.SaveChangesAsync();

                contents = await _context.AboutContents
                    .OrderBy(a => a.PageKey)
                    .ToListAsync();
            }

            return View(contents);
        }

        // GET: /AboutAdmin/Edit/{pageKey}
        public async Task<IActionResult> Edit(string? pageKey)
        {
            if (string.IsNullOrEmpty(pageKey))
            {
                return NotFound();
            }

            var content = await _context.AboutContents
                .FirstOrDefaultAsync(a => a.PageKey == pageKey);

            if (content == null)
            {
                // Eğer kayıt yoksa, yeni oluştur
                content = new AboutContent
                {
                    PageKey = pageKey,
                    Title = GetDefaultTitle(pageKey),
                    Content = "Bu sayfanın içeriği henüz eklenmemiştir.",
                    LastUpdated = DateTime.UtcNow
                };
                _context.AboutContents.Add(content);
                await _context.SaveChangesAsync();
            }

            return View(content);
        }

        // POST: /AboutAdmin/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string PageKey, string Title, string Content, string ImageUrl, string LinkUrl, string LinkText)
        {
            if (string.IsNullOrEmpty(PageKey))
            {
                TempData["ErrorMessage"] = "PageKey is required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Form'dan gelen TÜM değerleri direkt Request.Form'dan al
                var titleValue = Request.Form["Title"].ToString().Trim();
                var contentValue = Request.Form["Content"].ToString();
                var imageUrlValue = Request.Form["ImageUrl"].ToString().Trim();
                var linkUrlValue = Request.Form["LinkUrl"].ToString().Trim();
                var linkTextValue = Request.Form["LinkText"].ToString().Trim();
                
                // Entity'yi TRACKED olarak bul - Bu çok önemli!
                var existingContent = await _context.AboutContents
                    .FirstOrDefaultAsync(a => a.PageKey == PageKey);

                if (existingContent == null)
                {
                    TempData["ErrorMessage"] = "About content not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Tracked entity'nin property'lerini direkt güncelle - Bu kesinlikle çalışır!
                existingContent.Title = !string.IsNullOrEmpty(titleValue) ? titleValue : (Title ?? string.Empty);
                existingContent.Content = !string.IsNullOrEmpty(contentValue) ? contentValue : string.Empty;
                existingContent.ImageUrl = string.IsNullOrWhiteSpace(imageUrlValue) ? null : imageUrlValue;
                existingContent.LinkUrl = string.IsNullOrWhiteSpace(linkUrlValue) ? null : linkUrlValue;
                existingContent.LinkText = string.IsNullOrWhiteSpace(linkTextValue) ? null : linkTextValue;
                existingContent.LastUpdated = DateTime.UtcNow;
                
                // Değişiklikleri kaydet - Tracked entity olduğu için otomatik algılanır
                var savedCount = await _context.SaveChangesAsync();
                
                if (savedCount > 0)
                {
                    TempData["SuccessMessage"] = "About content has been updated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No changes were saved. Please try again.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AboutContentExists(Id))
                {
                    TempData["ErrorMessage"] = "About content not found. It may have been deleted.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Detaylı hata mesajı (debug için)
                var errorDetails = $"An error occurred while updating the content: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorDetails += $" Inner: {ex.InnerException.Message}";
                }
                TempData["ErrorMessage"] = errorDetails;
                return RedirectToAction(nameof(Edit), new { pageKey = PageKey });
            }
        }

        private bool AboutContentExists(int id)
        {
            return _context.AboutContents.Any(e => e.Id == id);
        }

        private string GetDefaultTitle(string pageKey)
        {
            return pageKey switch
            {
                "PastEvents" => "Past Events",
                "AboutOrganizer" => "About Organizer",
                "Testimonials" => "Testimonials / Reviews",
                _ => "About"
            };
        }
    }
}

