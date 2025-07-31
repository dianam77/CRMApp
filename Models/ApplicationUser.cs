using CRMApp.Models;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
