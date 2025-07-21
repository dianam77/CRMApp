using CRMApp.Constants;
using CRMApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CRMApp.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    public class RolesController : Controller
    {
        private readonly CRMAppDbContext _context;

        public RolesController(CRMAppDbContext context)
        {
            _context = context;
        }

        // GET: /Roles/Manage
        public async Task<IActionResult> Manage()
        {
            var roles = await _context.Roles
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .ToListAsync();

            return View(roles);
        }
    }
}
