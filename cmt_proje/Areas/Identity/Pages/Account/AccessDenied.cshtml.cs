using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using cmt_proje.Infrastructure.Data;
using System.Text.RegularExpressions;
using System.Net;

namespace cmt_proje.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        private readonly ConferenceDbContext _context;

        public AccessDeniedModel(ConferenceDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            // Try to get conferenceId from ReturnUrl
            var returnUrl = Request.Query["ReturnUrl"].ToString();
            if (!string.IsNullOrEmpty(returnUrl))
            {
                // Decode URL if needed
                var decodedUrl = WebUtility.UrlDecode(returnUrl);
                
                // Try to extract conferenceId from URL patterns like:
                // /Submissions?conferenceId=13
                // /Submissions?conferenceId=13&trackId=5
                var match = Regex.Match(decodedUrl, @"[?&]conferenceId=(\d+)", RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int confId))
                {
                    var conference = await _context.Conferences
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == confId);
                    
                    if (conference != null)
                    {
                        ViewData["ActiveConferenceId"] = confId;
                        ViewData["ActiveConferenceAcronym"] = conference.Acronym;
                        ViewData["Conference"] = conference;
                        
                        // Set conference-specific body class for gradient
                        string acronym = conference.Acronym?.ToUpper() ?? "";
                        string bodyClass = "dashboard-page";
                        
                        if (acronym.Contains("ICINSE"))
                        {
                            bodyClass = "conf-icinse";
                        }
                        else if (acronym.Contains("ITCHS"))
                        {
                            bodyClass = "conf-itchs";
                        }
                        else if (acronym.Contains("ITWCCST") || acronym.Contains("ITW"))
                        {
                            bodyClass = "conf-itwccst";
                        }
                        
                        ViewData["BodyClass"] = bodyClass;
                        return;
                    }
                }
            }
            
            // Default body class if no conference found
            ViewData["BodyClass"] = "dashboard-page";
        }
    }
}

