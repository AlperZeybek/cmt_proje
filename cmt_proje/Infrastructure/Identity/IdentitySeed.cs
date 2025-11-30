using System;
using System.Threading.Tasks;
using cmt_proje.Core.Constants;
using cmt_proje.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace cmt_proje.Infrastructure.Identity
{
    public static class IdentitySeed
    {
        public static async Task SeedRolesAndChairAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                                                   .CreateLogger("IdentitySeed");

            string[] roles = { AppRoles.Chair, AppRoles.Reviewer, AppRoles.Author };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                    {
                        logger.LogWarning("Role {Role} oluşturulamadı: {Errors}",
                            role, string.Join(",", result.Errors));
                    }
                }
            }

            // Mevcut hesabını Chair yapalım (sende görünen mail)
            const string chairEmail = "alper123@gmail.com";

            var chairUser = await userManager.FindByEmailAsync(chairEmail);
            if (chairUser != null)
            {
                if (!await userManager.IsInRoleAsync(chairUser, AppRoles.Chair))
                {
                    var result = await userManager.AddToRoleAsync(chairUser, AppRoles.Chair);
                    if (!result.Succeeded)
                    {
                        logger.LogWarning("Chair rolü eklenemedi: {Errors}",
                            string.Join(",", result.Errors));
                    }
                }
            }
            else
            {
                logger.LogWarning("Chair seed için {Email} bulunamadı. Daha sonra manuel atama yapabilirsin.", chairEmail);
            }
        }
    }
}
