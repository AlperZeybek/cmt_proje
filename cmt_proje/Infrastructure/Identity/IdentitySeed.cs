using System;
using System.Linq;
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

            // Eski Reviewer rolünü kaldır (artık kullanılmıyor)
            const string reviewerRoleName = "Reviewer";
            if (await roleManager.RoleExistsAsync(reviewerRoleName))
            {
                var reviewerRole = await roleManager.FindByNameAsync(reviewerRoleName);
                if (reviewerRole != null)
                {
                    // Reviewer rolüne atanmış tüm kullanıcıları bul ve rolden çıkar
                    var usersInReviewerRole = await userManager.GetUsersInRoleAsync(reviewerRoleName);
                    foreach (var user in usersInReviewerRole)
                    {
                        var removeResult = await userManager.RemoveFromRoleAsync(user, reviewerRoleName);
                        if (removeResult.Succeeded)
                        {
                            logger.LogInformation("User {Email} removed from Reviewer role.", user.Email);
                        }
                        else
                        {
                            logger.LogWarning("Failed to remove user {Email} from Reviewer role: {Errors}",
                                user.Email, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                        }
                    }

                    // Reviewer rolünü sil
                    var deleteResult = await roleManager.DeleteAsync(reviewerRole);
                    if (deleteResult.Succeeded)
                    {
                        logger.LogInformation("Reviewer role has been removed from database.");
                    }
                    else
                    {
                        logger.LogWarning("Failed to delete Reviewer role: {Errors}",
                            string.Join(", ", deleteResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            // Sadece Chair ve Author rollerini oluştur
            string[] roles = { AppRoles.Chair, AppRoles.Author };

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
                    else
                    {
                        logger.LogInformation("Role {Role} created successfully.", role);
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
