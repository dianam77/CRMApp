using Microsoft.AspNetCore.Identity;
using System;

namespace CRMApp.Models
{
    public class UserRole : IdentityUserRole<Guid>
    {
        public ApplicationUser User { get; set; }
        public ApplicationRole Role { get; set; }
    }
}
