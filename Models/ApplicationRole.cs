using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System;

namespace CRMApp.Models
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
