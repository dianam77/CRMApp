using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMApp.Data;
using CRMApp.Models;
using CRMApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CRMApp.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CustomerIndividualController : Controller
    {
        private readonly CRMAppDbContext _context;

        public CustomerIndividualController(CRMAppDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Index()
        {
            var individuals = await _context.CustomerIndividuals
                .Include(i => i.Emails)
                .Include(i => i.ContactPhones)
                .Include(i => i.Addresses)
                .ToListAsync();

            return View(individuals);
        }

        // GET: Create
        public IActionResult Create()
        {
            var vm = new CustomerIndividualViewModel
            {
                Emails = new List<EmailViewModel> { new EmailViewModel() },
                ContactPhones = new List<PhoneViewModel> { new PhoneViewModel() },
                Addresses = new List<AddressViewModel> { new AddressViewModel() },
                GenderList = GetGenderList(),
                MaritalStatusList = GetMaritalStatusList()
            };

            return View(vm);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerIndividualViewModel vm)
        {
            // حذف آیتم‌های خالی از لیست‌ها قبل از اعتبارسنجی
            vm.Emails = vm.Emails?.Where(e => !string.IsNullOrWhiteSpace(e.EmailAddress)).ToList() ?? new List<EmailViewModel>();
            vm.ContactPhones = vm.ContactPhones?.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList() ?? new List<PhoneViewModel>();
            vm.Addresses = vm.Addresses?.Where(a => !string.IsNullOrWhiteSpace(a.FullAddress)).ToList() ?? new List<AddressViewModel>();

            // پاک کردن ModelState برای آیتم‌های حذف شده
            ClearModelStateForList(nameof(vm.Emails), vm.Emails.Count);
            ClearModelStateForList(nameof(vm.ContactPhones), vm.ContactPhones.Count);
            ClearModelStateForList(nameof(vm.Addresses), vm.Addresses.Count);

            vm.GenderList = GetGenderList();
            vm.MaritalStatusList = GetMaritalStatusList();

            if (!ModelState.IsValid)
                return View(vm);

            var individual = new CustomerIndividual
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                FatherName = vm.FatherName,
                BirthDate = vm.BirthDate,
                NationalCode = vm.NationalCode,
                IdentityNumber = vm.IdentityNumber,
                Gender = vm.Gender,
                MaritalStatus = vm.MaritalStatus
            };

            _context.CustomerIndividuals.Add(individual);
            await _context.SaveChangesAsync();

            await AddEmailsAsync(individual.CustomerId, vm.Emails);
            await AddPhonesAsync(individual.CustomerId, vm.ContactPhones);
            await AddAddressesAsync(individual.CustomerId, vm.Addresses);

            return RedirectToAction(nameof(Index));
        }
        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var individual = await _context.CustomerIndividuals
                .Include(i => i.Emails)
                .Include(i => i.ContactPhones)
                .Include(i => i.Addresses)
                .FirstOrDefaultAsync(i => i.CustomerId == id);

            if (individual == null)
                return NotFound();

            var vm = new CustomerIndividualViewModel
            {
                CustomerId = individual.CustomerId,
                FirstName = individual.FirstName,
                LastName = individual.LastName,
                FatherName = individual.FatherName,
                BirthDate = individual.BirthDate,
                NationalCode = individual.NationalCode,
                IdentityNumber = individual.IdentityNumber,
                Gender = individual.Gender,
                MaritalStatus = individual.MaritalStatus,

                Emails = individual.Emails?.Select(e => new EmailViewModel
                {
                    EmailId = e.EmailId,
                    EmailAddress = e.EmailAddress,
                    EmailType = e.EmailType,
                    IsPrimary = e.IsPrimary
                }).ToList() ?? new List<EmailViewModel>(),

                ContactPhones = individual.ContactPhones?.Select(p => new PhoneViewModel
                {
                    PhoneId = p.PhoneId,
                    PhoneNumber = p.PhoneNumber,
                    PhoneType = p.PhoneType,
                    Extension = p.Extension
                }).ToList() ?? new List<PhoneViewModel>(),

                Addresses = individual.Addresses?.Select(a => new AddressViewModel
                {
                    AddressId = a.AddressId,
                    FullAddress = a.FullAddress,
                    City = a.City,
                    Province = a.Province,
                    PostalCode = a.PostalCode,
                    AddressType = a.AddressType
                }).ToList() ?? new List<AddressViewModel>(),

                GenderList = GetGenderList(),
                MaritalStatusList = GetMaritalStatusList()
            };

            // ✅ اضافه کردن ردیف خالی در صورت نبود داده برای نمایش در فرم
            if (!vm.Emails.Any())
                vm.Emails.Add(new EmailViewModel());

            if (!vm.ContactPhones.Any())
                vm.ContactPhones.Add(new PhoneViewModel());

            if (!vm.Addresses.Any())
                vm.Addresses.Add(new AddressViewModel());

            return View(vm);
        }



        /// POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerIndividualViewModel vm)
        {
            if (id != vm.CustomerId)
                return NotFound();

            // حذف مقادیر خالی
            vm.Emails = vm.Emails?.Where(e => !string.IsNullOrWhiteSpace(e.EmailAddress)).ToList() ?? new List<EmailViewModel>();
            vm.ContactPhones = vm.ContactPhones?.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList() ?? new List<PhoneViewModel>();
            vm.Addresses = vm.Addresses?.Where(a => !string.IsNullOrWhiteSpace(a.FullAddress)).ToList() ?? new List<AddressViewModel>();

            // پاک‌سازی ModelState برای مقادیر حذف شده
            ClearModelStateForList(nameof(vm.Emails), vm.Emails.Count);
            ClearModelStateForList(nameof(vm.ContactPhones), vm.ContactPhones.Count);
            ClearModelStateForList(nameof(vm.Addresses), vm.Addresses.Count);

            // تنظیم مجدد لیست‌های DropDown
            vm.GenderList = GetGenderList();
            vm.MaritalStatusList = GetMaritalStatusList();

            if (!ModelState.IsValid)
                return View(vm);

            var existing = await _context.CustomerIndividuals
                .Include(i => i.Emails)
                .Include(i => i.ContactPhones)
                .Include(i => i.Addresses)
                .FirstOrDefaultAsync(i => i.CustomerId == id);

            if (existing == null)
                return NotFound();

            // به‌روزرسانی مشخصات فردی
            existing.FirstName = vm.FirstName;
            existing.LastName = vm.LastName;
            existing.FatherName = vm.FatherName;
            existing.BirthDate = vm.BirthDate;
            existing.NationalCode = vm.NationalCode;
            existing.IdentityNumber = vm.IdentityNumber;
            existing.Gender = vm.Gender;
            existing.MaritalStatus = vm.MaritalStatus;

            // به‌روزرسانی لیست‌های فرزند
            UpdateEmails(existing, vm.Emails);
            UpdatePhones(existing, vm.ContactPhones);
            UpdateAddresses(existing, vm.Addresses);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int id)
        {
            var individual = await _context.CustomerIndividuals.FindAsync(id);
            if (individual == null)
                return NotFound();

            return View(individual);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var individual = await _context.CustomerIndividuals
                .Include(i => i.Emails)
                .Include(i => i.ContactPhones)
                .Include(i => i.Addresses)
                .FirstOrDefaultAsync(i => i.CustomerId == id);

            if (individual != null)
            {
                if (individual.Emails.Any())
                    _context.Emails.RemoveRange(individual.Emails);

                if (individual.ContactPhones.Any())
                    _context.ContactPhones.RemoveRange(individual.ContactPhones);

                if (individual.Addresses.Any())
                    _context.Addresses.RemoveRange(individual.Addresses);

                _context.CustomerIndividuals.Remove(individual);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helpers

        private List<SelectListItem> GetGenderList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "مرد", Text = "مرد" },
                new SelectListItem { Value = "زن", Text = "زن" }
            };
        }

        private List<SelectListItem> GetMaritalStatusList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "مجرد", Text = "مجرد" },
                new SelectListItem { Value = "متاهل", Text = "متاهل" }
            };
        }

        private async Task AddEmailsAsync(int individualId, List<EmailViewModel> emails)
        {
            var validEmails = emails?
                .Where(e => !string.IsNullOrWhiteSpace(e.EmailAddress))
                .Select(e => new Email
                {
                    IndividualCustomerId = individualId,
                    EmailAddress = e.EmailAddress,
                    EmailType = e.EmailType,
                    IsPrimary = e.IsPrimary
                }).ToList();

            if (validEmails?.Any() == true)
            {
                _context.Emails.AddRange(validEmails);
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddPhonesAsync(int individualId, List<PhoneViewModel> phones)
        {
            var validPhones = phones?
                .Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber))
                .Select(p => new ContactPhone
                {
                    IndividualCustomerId = individualId,
                    PhoneNumber = p.PhoneNumber,
                    PhoneType = p.PhoneType,
                    Extension = p.Extension
                }).ToList();

            if (validPhones?.Any() == true)
            {
                _context.ContactPhones.AddRange(validPhones);
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddAddressesAsync(int individualId, List<AddressViewModel> addresses)
        {
            var validAddresses = addresses?
                .Where(a => !string.IsNullOrWhiteSpace(a.FullAddress))
                .Select(a => new Address
                {
                    IndividualCustomerId = individualId,
                    AddressType = a.AddressType,
                    Province = a.Province,
                    City = a.City,
                    PostalCode = a.PostalCode,
                    FullAddress = a.FullAddress
                }).ToList();

            if (validAddresses?.Any() == true)
            {
                _context.Addresses.AddRange(validAddresses);
                await _context.SaveChangesAsync();
            }
        }

        private void UpdateEmails(CustomerIndividual existing, List<EmailViewModel> emails)
        {
            // حذف ایمیل‌هایی که دیگر وجود ندارند
            var toRemove = existing.Emails.Where(e => !emails.Any(vm => vm.EmailId == e.EmailId)).ToList();
            _context.Emails.RemoveRange(toRemove);

            // افزودن و یا به‌روزرسانی ایمیل‌ها
            foreach (var vm in emails)
            {
                var email = existing.Emails.FirstOrDefault(e => e.EmailId == vm.EmailId);
                if (email == null)
                {
                    existing.Emails.Add(new Email
                    {
                        IndividualCustomerId = existing.CustomerId,
                        EmailAddress = vm.EmailAddress,
                        EmailType = vm.EmailType,
                        IsPrimary = vm.IsPrimary
                    });
                }
                else
                {
                    email.EmailAddress = vm.EmailAddress;
                    email.EmailType = vm.EmailType;
                    email.IsPrimary = vm.IsPrimary;
                }
            }
        }

        private void UpdatePhones(CustomerIndividual existing, List<PhoneViewModel> phones)
        {
            var toRemove = existing.ContactPhones.Where(p => !phones.Any(vm => vm.PhoneId == p.PhoneId)).ToList();
            _context.ContactPhones.RemoveRange(toRemove);

            foreach (var vm in phones)
            {
                var phone = existing.ContactPhones.FirstOrDefault(p => p.PhoneId == vm.PhoneId);
                if (phone == null)
                {
                    existing.ContactPhones.Add(new ContactPhone
                    {
                        IndividualCustomerId = existing.CustomerId,
                        PhoneNumber = vm.PhoneNumber,
                        PhoneType = vm.PhoneType,
                        Extension = vm.Extension
                    });
                }
                else
                {
                    phone.PhoneNumber = vm.PhoneNumber;
                    phone.PhoneType = vm.PhoneType;
                    phone.Extension = vm.Extension;
                }
            }
        }

        private void UpdateAddresses(CustomerIndividual existing, List<AddressViewModel> addresses)
        {
            var toRemove = existing.Addresses.Where(a => !addresses.Any(vm => vm.AddressId == a.AddressId)).ToList();
            _context.Addresses.RemoveRange(toRemove);

            foreach (var vm in addresses)
            {
                var addr = existing.Addresses.FirstOrDefault(a => a.AddressId == vm.AddressId);
                if (addr == null)
                {
                    existing.Addresses.Add(new Address
                    {
                        IndividualCustomerId = existing.CustomerId,
                        AddressType = vm.AddressType,
                        Province = vm.Province,
                        City = vm.City,
                        PostalCode = vm.PostalCode,
                        FullAddress = vm.FullAddress
                    });
                }
                else
                {
                    addr.AddressType = vm.AddressType;
                    addr.Province = vm.Province;
                    addr.City = vm.City;
                    addr.PostalCode = vm.PostalCode;
                    addr.FullAddress = vm.FullAddress;
                }
            }
        }

        private void ClearModelStateForList(string propertyName, int validItemCount)
        {
            var keysToRemove = ModelState.Keys
                .Where(k => k.StartsWith(propertyName + "["))
                .Where(k =>
                {
                    var start = k.IndexOf('[') + 1;
                    var end = k.IndexOf(']');
                    if (start >= 0 && end > start)
                    {
                        var indexStr = k.Substring(start, end - start);
                        if (int.TryParse(indexStr, out int index))
                        {
                            return index >= validItemCount;
                        }
                    }
                    return false;
                }).ToList();

            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }
        }

        #endregion
    }
}
