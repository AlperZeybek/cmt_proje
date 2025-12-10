using System.Threading.Tasks;

namespace cmt_proje.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    }
}

