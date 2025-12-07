using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cmt_proje.Core.Constants;
using cmt_proje.Core.Entities;
using cmt_proje.Infrastructure.Data;
using cmt_proje.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace cmt_proje.Controllers
{
    [Authorize(Roles = AppRoles.Chair)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ConferenceDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, ConferenceDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .Where(u => !string.IsNullOrWhiteSpace(u.FullName)) // Boş FullName'li kullanıcıları filtrele
                .OrderBy(u => u.FullName)
                .Select(u => new UserListViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName ?? string.Empty,
                    Affiliation = u.Affiliation ?? string.Empty,
                    Faculty = u.Faculty ?? string.Empty,
                    Department = u.Department ?? string.Empty
                })
                .ToListAsync();

            return View(users);
        }

        // GET: /Admin/ExportUsersToExcel
        public async Task<IActionResult> ExportUsersToExcel()
        {
            var users = await _userManager.Users
                .Where(u => !string.IsNullOrWhiteSpace(u.FullName)) // Boş FullName'li kullanıcıları filtrele
                .OrderBy(u => u.FullName)
                .Select(u => new UserListViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName ?? string.Empty,
                    Affiliation = u.Affiliation ?? string.Empty,
                    Faculty = u.Faculty ?? string.Empty,
                    Department = u.Department ?? string.Empty
                })
                .ToListAsync();

            // EPPlus lisans ayarı (non-commercial kullanım için)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");

                // Başlık satırı (Satır 1)
                worksheet.Cells[1, 1].Value = "Full Name";
                worksheet.Cells[1, 2].Value = "Affiliation / University Name";
                worksheet.Cells[1, 3].Value = "Faculty";
                worksheet.Cells[1, 4].Value = "Department";

                // Başlık stilini ayarla (yeşil arka plan)
                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(16, 185, 129)); // Yeşil renk
                    range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Veri satırlarını ekle (Satır 2'den başla)
                for (int i = 0; i < users.Count; i++)
                {
                    var user = users[i];
                    int row = i + 2; // İlk veri satırı 2

                    worksheet.Cells[row, 1].Value = user.FullName;
                    worksheet.Cells[row, 2].Value = user.Affiliation;
                    worksheet.Cells[row, 3].Value = user.Faculty;
                    worksheet.Cells[row, 4].Value = user.Department;

                    // Hücre kenarlıkları
                    worksheet.Cells[row, 1, row, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }

                // Sütun genişliklerini ayarla
                worksheet.Column(1).Width = 25; // Full Name
                worksheet.Column(2).Width = 35; // Affiliation
                worksheet.Column(3).Width = 30; // Faculty
                worksheet.Column(4).Width = 30; // Department

                // Dosya adı
                var fileName = $"Users_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var fileBytes = package.GetAsByteArray();

                return File(fileBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    fileName);
            }
        }
    }
}

