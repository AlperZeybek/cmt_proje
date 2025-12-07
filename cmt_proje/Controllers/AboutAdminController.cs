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
        public async Task<IActionResult> Edit([Bind("Id,PageKey,Title,Content,ImageUrl,LinkUrl,LinkText,CreatedAt")] AboutContent content)
        {
            if (string.IsNullOrEmpty(content?.PageKey))
            {
                return RedirectToAction(nameof(Index));
            }

            // Remove readonly fields from ModelState validation
            ModelState.Remove("PageKey");
            ModelState.Remove("CreatedAt");
            
            // Remove URL validation errors for empty optional fields
            if (string.IsNullOrWhiteSpace(content.ImageUrl))
            {
                ModelState.Remove("ImageUrl");
            }
            if (string.IsNullOrWhiteSpace(content.LinkUrl))
            {
                ModelState.Remove("LinkUrl");
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    AboutContent existingContent = null;
                    
                    // First try to find by Id if it's provided and valid
                    if (content.Id > 0)
                    {
                        existingContent = await _context.AboutContents
                            .FirstOrDefaultAsync(a => a.Id == content.Id);
                    }
                    
                    // If not found by Id, try to find by PageKey
                    if (existingContent == null)
                    {
                        existingContent = await _context.AboutContents
                            .FirstOrDefaultAsync(a => a.PageKey == content.PageKey);
                    }
                    
                    if (existingContent == null)
                    {
                        TempData["ErrorMessage"] = "About content not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Update all properties that can be changed
                    existingContent.Title = content.Title ?? string.Empty;
                    existingContent.Content = content.Content ?? string.Empty;
                    existingContent.ImageUrl = string.IsNullOrWhiteSpace(content.ImageUrl) ? null : content.ImageUrl;
                    existingContent.LinkUrl = string.IsNullOrWhiteSpace(content.LinkUrl) ? null : content.LinkUrl;
                    existingContent.LinkText = string.IsNullOrWhiteSpace(content.LinkText) ? null : content.LinkText;
                    existingContent.LastUpdated = DateTime.UtcNow;

                    // Save changes - Entity Framework automatically tracks changes to tracked entities
                    var savedCount = await _context.SaveChangesAsync();
                    
                    // Verify the save was successful
                    if (savedCount == 0)
                    {
                        TempData["ErrorMessage"] = "No changes were saved. Please try again.";
                        return RedirectToAction(nameof(Edit), new { pageKey = content.PageKey });
                    }
                    
                    TempData["SuccessMessage"] = "About content has been updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AboutContentExists(content.Id))
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
                    TempData["ErrorMessage"] = "An error occurred while updating the content.";
                    return RedirectToAction(nameof(Edit), new { pageKey = content.PageKey });
                }
            }

            // ModelState geçersizse, veriyi yeniden yükle ve View'a dön
            var existingContentForView = await _context.AboutContents
                .FirstOrDefaultAsync(a => a.PageKey == content.PageKey);
            
            if (existingContentForView != null)
            {
                // Mevcut değerleri koru, sadece gönderilen değerleri güncelle
                existingContentForView.Title = content.Title;
                existingContentForView.Content = content.Content;
                existingContentForView.ImageUrl = content.ImageUrl;
                existingContentForView.LinkUrl = content.LinkUrl;
                existingContentForView.LinkText = content.LinkText;
                return View(existingContentForView);
            }
            
            return RedirectToAction(nameof(Index));
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

