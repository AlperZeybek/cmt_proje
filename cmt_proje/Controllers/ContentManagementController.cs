using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmt_proje.Core.Entities;
using cmt_proje.Core.Constants;
using cmt_proje.Infrastructure.Data;
using cmt_proje.Models;

namespace cmt_proje.Controllers
{
    [Authorize(Roles = AppRoles.Chair)]
    public class ContentManagementController : Controller
    {
        private readonly ConferenceDbContext _context;

        public ContentManagementController(ConferenceDbContext context)
        {
            _context = context;
        }

        // GET: ContentManagement/Index
        public IActionResult Index()
        {
            ViewData["Title"] = "Content Management";
            return View();
        }

        // GET: ContentManagement/PageBuilder?pageKey=Home
        public async Task<IActionResult> PageBuilder(string? pageKey = "Home")
        {
            var blocks = await _context.PageContentBlocks
                .Where(b => b.PageKey == pageKey && b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();

            ViewBag.PageKey = pageKey;
            ViewBag.Blocks = blocks;
            ViewData["Title"] = $"Page Builder - {pageKey}";
            return View(blocks);
        }

        // GET: ContentManagement/GetBlock?id=1
        public async Task<IActionResult> GetBlock(int id)
        {
            var block = await _context.PageContentBlocks.FindAsync(id);
            if (block == null)
            {
                return Json(new { success = false, message = "Block not found" });
            }

            return Json(new
            {
                success = true,
                block = new
                {
                    id = block.Id,
                    blockType = block.BlockType,
                    title = block.Title,
                    content = block.Content,
                    imageUrl = block.ImageUrl,
                    linkUrl = block.LinkUrl,
                    linkText = block.LinkText,
                    cssClass = block.CssClass
                }
            });
        }

        // POST: ContentManagement/AddBlock
        [HttpPost]
        [IgnoreAntiforgeryToken] // JSON istekleri için
        public async Task<IActionResult> AddBlock([FromBody] PageContentBlockViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            // Get max order value for this page, or 0 if no blocks exist
            var existingBlocks = await _context.PageContentBlocks
                .Where(b => b.PageKey == model.PageKey)
                .ToListAsync();
            
            var maxOrder = existingBlocks.Any() ? existingBlocks.Max(b => b.DisplayOrder) : 0;

            var block = new PageContentBlock
            {
                PageKey = model.PageKey,
                BlockType = model.BlockType,
                Title = model.Title,
                Content = model.Content,
                ImageUrl = model.ImageUrl,
                LinkUrl = model.LinkUrl,
                LinkText = model.LinkText,
                CssClass = model.CssClass,
                DisplayOrder = maxOrder + 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PageContentBlocks.Add(block);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = block.Id, message = "Block added successfully" });
        }

        // POST: ContentManagement/UpdateBlock
        [HttpPost]
        [IgnoreAntiforgeryToken] // JSON istekleri için
        public async Task<IActionResult> UpdateBlock([FromBody] PageContentBlockViewModel model)
        {
            if (!ModelState.IsValid || model.Id == null)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var block = await _context.PageContentBlocks.FindAsync(model.Id);
            if (block == null)
            {
                return Json(new { success = false, message = "Block not found" });
            }

            block.BlockType = model.BlockType;
            block.Title = model.Title;
            block.Content = model.Content;
            block.ImageUrl = model.ImageUrl;
            block.LinkUrl = model.LinkUrl;
            block.LinkText = model.LinkText;
            block.CssClass = model.CssClass;
            block.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Block updated successfully" });
        }

        // POST: ContentManagement/DeleteBlock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBlock(int id)
        {
            var block = await _context.PageContentBlocks.FindAsync(id);
            if (block == null)
            {
                return Json(new { success = false, message = "Block not found" });
            }

            _context.PageContentBlocks.Remove(block);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Block deleted successfully" });
        }

        // POST: ContentManagement/ReorderBlocks
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderBlocks([FromBody] List<BlockOrderViewModel> orders)
        {
            foreach (var order in orders)
            {
                var block = await _context.PageContentBlocks.FindAsync(order.Id);
                if (block != null)
                {
                    block.DisplayOrder = order.Order;
                    block.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Blocks reordered successfully" });
        }

        // GET: ContentManagement/NavbarManagement
        public async Task<IActionResult> NavbarManagement()
        {
            var items = await _context.NavigationItems
                .Where(n => n.ParentId == null)
                .OrderBy(n => n.DisplayOrder)
                .ToListAsync();

            // Load children for each item
            foreach (var item in items)
            {
                var children = await _context.NavigationItems
                    .Where(n => n.ParentId == item.Id)
                    .OrderBy(n => n.DisplayOrder)
                    .ToListAsync();
                // Note: NavigationItem doesn't have Children collection, we'll handle it in view
            }

            ViewData["Title"] = "Navbar Management";
            return View(items);
        }

        // POST: ContentManagement/AddNavItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNavItem([FromBody] NavigationItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var maxOrder = await _context.NavigationItems
                .Where(n => n.ParentId == model.ParentId)
                .Select(n => (int?)n.DisplayOrder)
                .DefaultIfEmpty(0)
                .MaxAsync();

            var item = new NavigationItem
            {
                Label = model.Label,
                Url = model.Url,
                Controller = model.Controller,
                Action = model.Action,
                Area = model.Area,
                Icon = model.Icon,
                ParentId = model.ParentId,
                DisplayOrder = (maxOrder ?? 0) + 1,
                IsActive = true,
                IsDropdown = model.IsDropdown,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.NavigationItems.Add(item);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = item.Id, message = "Navigation item added successfully" });
        }

        // POST: ContentManagement/UpdateNavItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNavItem([FromBody] NavigationItemViewModel model)
        {
            if (!ModelState.IsValid || model.Id == null)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var item = await _context.NavigationItems.FindAsync(model.Id);
            if (item == null)
            {
                return Json(new { success = false, message = "Navigation item not found" });
            }

            item.Label = model.Label;
            item.Url = model.Url;
            item.Controller = model.Controller;
            item.Action = model.Action;
            item.Area = model.Area;
            item.Icon = model.Icon;
            item.IsDropdown = model.IsDropdown;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Navigation item updated successfully" });
        }

        // POST: ContentManagement/DeleteNavItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNavItem(int id)
        {
            var item = await _context.NavigationItems.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Navigation item not found" });
            }

            // Check if has children
            var hasChildren = await _context.NavigationItems.AnyAsync(n => n.ParentId == id);
            if (hasChildren)
            {
                return Json(new { success = false, message = "Cannot delete item with children. Delete children first." });
            }

            _context.NavigationItems.Remove(item);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Navigation item deleted successfully" });
        }

        // POST: ContentManagement/ReorderNavItems
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderNavItems([FromBody] List<NavItemOrderViewModel> orders)
        {
            foreach (var order in orders)
            {
                var item = await _context.NavigationItems.FindAsync(order.Id);
                if (item != null)
                {
                    item.DisplayOrder = order.Order;
                    item.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Navigation items reordered successfully" });
        }

        // GET: ContentManagement/GetNavItem?id=1
        public async Task<IActionResult> GetNavItem(int id)
        {
            var item = await _context.NavigationItems.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Navigation item not found" });
            }

            return Json(new
            {
                success = true,
                item = new
                {
                    id = item.Id,
                    label = item.Label,
                    url = item.Url,
                    controller = item.Controller,
                    action = item.Action,
                    area = item.Area,
                    icon = item.Icon,
                    isDropdown = item.IsDropdown
                }
            });
        }
    }
}

