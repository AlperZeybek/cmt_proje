using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using cmt_proje.Core.Constants;

namespace cmt_proje.Controllers
{
    [Authorize(Roles = AppRoles.Chair)]
    public class AdminPanelController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Conference Management Panel";
            ViewData["BodyClass"] = "dashboard-page";
            return View();
        }
    }
}

