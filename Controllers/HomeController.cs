using CRMApp.Constants;
using CRMApp.Data;
using CRMApp.DTOs;
using CRMApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CRMApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly CRMAppDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(
            CRMAppDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: فرم ورود
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: پردازش فرم ورود
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: true, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور اشتباه است");
                return View(model);
            }

            return RedirectToAction("Dashboard");
        }

        // GET: فرم ثبت نام
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterDto());
        }

        // POST: ثبت نام کاربر جدید
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _userManager.FindByNameAsync(model.Username) != null)
            {
                ModelState.AddModelError("", "نام کاربری از قبل وجود دارد.");
                return View(model);
            }

            var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == model.RoleName);
            if (role == null)
            {
                ModelState.AddModelError("", "نقش انتخاب شده معتبر نیست.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email  // مطمئن شوید مدل RegisterDto این فیلد را دارد
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, role.Name);

            return RedirectToAction("Index");
        }

        // GET: داشبورد (نیاز به احراز هویت دارد)
        [HttpGet]
        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        // POST: خروج از سیستم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}
