using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Smartway.FileLoaderApi.Entities;
using System.Security.Claims;

namespace Smartway.FileLoaderApi.Extensions;

public static class UserManagerExtensions
{
    public static async Task<AppUser> FindByEmailFromClaimsPrincipal(this UserManager<AppUser> userManager,
        ClaimsPrincipal user)
    {
        return await userManager.Users
            .SingleOrDefaultAsync(x => x.Email == user.FindFirstValue(ClaimTypes.Email));
    }

    public static async Task<string> GetUserIdByEmailFromClaimsPrincipal(this UserManager<AppUser> userManager,
        ClaimsPrincipal user)
    {
        return await userManager.Users
            .Where(x => x.Email == user.FindFirstValue(ClaimTypes.Email))
            .Select(x => x.Id)
            .SingleOrDefaultAsync();
    }
}
