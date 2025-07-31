using CRMApp.Constants;
using CRMApp.Data;
using CRMApp.Models;
using CRMApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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

        // لیست نقش‌ها با کاربران مربوطه
        public async Task<IActionResult> Manage()
        {
            var roles = await _context.Roles
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .ToListAsync();

            return View(roles);
        }

        // صفحه ویرایش نقش
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out Guid guidId))
                return NotFound();

            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == guidId);

            if (role == null)
                return NotFound();

            var users = await _context.Users.ToListAsync();

            var model = new RoleEditViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Users = users.Select(u => new UserCheckboxViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    IsSelected = role.UserRoles.Any(ur => ur.UserId == u.Id)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RoleEditViewModel model)
        {
            if (id != model.RoleId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                return NotFound();

            // بررسی تکراری نبودن نام نقش (غیر از خودش)
            var normalizedNewName = model.RoleName.ToUpper();
            var existingRoleWithName = await _context.Roles
                .FirstOrDefaultAsync(r => r.NormalizedName == normalizedNewName && r.Id != id);

            if (existingRoleWithName != null)
            {
                ModelState.AddModelError("RoleName", "نام نقش وارد شده تکراری است.");
                return View(model);
            }

            // بروز رسانی نام نقش
            role.Name = model.RoleName;
            role.NormalizedName = normalizedNewName;

            var selectedUserIds = model.Users.Where(u => u.IsSelected).Select(u => u.UserId).ToList();

            // حذف نقش کاربران که دیگر انتخاب نشده‌اند
            var userRolesList = role.UserRoles.ToList();
            foreach (var userRole in userRolesList)
            {
                if (!selectedUserIds.Contains(userRole.UserId))
                {
                    role.UserRoles.Remove(userRole);
                }
            }

            // اضافه کردن نقش به کاربران جدید
            var existingUserIds = role.UserRoles.Select(ur => ur.UserId).ToList();
            var newUserIds = selectedUserIds.Except(existingUserIds).ToList();

            foreach (var userId in newUserIds)
            {
                role.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id
                });
            }

            try
            {
                _context.Update(role);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "خطایی هنگام ذخیره نقش رخ داده است.");
                return View(model);
            }

            return RedirectToAction(nameof(Manage));
        }
    }
}
