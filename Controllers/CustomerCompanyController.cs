using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMApp.Models;
using CRMApp.Data;
using CRMApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace CRMApp.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CustomerCompanyController : Controller
    {
        private readonly CRMAppDbContext _context;

        public CustomerCompanyController(CRMAppDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Index()
        {
            var companies = await _context.CustomerCompanies
                .Include(c => c.CustomerCompanyRelations).ThenInclude(r => r.IndividualCustomer)
                .Include(c => c.Emails)
                .Include(c => c.ContactPhones)
                .Include(c => c.Addresses)
                .ToListAsync();

            return View(companies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var company = await _context.CustomerCompanies
                .Include(c => c.CustomerCompanyRelations).ThenInclude(r => r.IndividualCustomer)
                .Include(c => c.Emails)
                .Include(c => c.ContactPhones)
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            return company == null ? NotFound() : View(company);
        }

        public async Task<IActionResult> Create()
        {
            var individuals = await _context.CustomerIndividuals.ToListAsync();

            var vm = new CompanyCreateViewModel
            {
                IndividualCustomers = individuals.Any()
                    ? individuals.Select(c => new IndividualCustomer
                    {
                        CustomerId = c.CustomerId,
                        FullName = $"{c.FirstName} {c.LastName}"
                    }).ToList()
                    : new List<IndividualCustomer> { new IndividualCustomer { CustomerId = 0, FullName = "هیچ مشتری حقیقی موجود نیست" } },

                Emails = new List<EmailViewModel> { new EmailViewModel() },
                ContactPhones = new List<PhoneViewModel> { new PhoneViewModel() },
                Addresses = new List<AddressViewModel> { new AddressViewModel() }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyCreateViewModel vm)
        {
            // فقط آیتم‌های دارای مقدار معتبر را نگه دار
            vm.Emails = vm.Emails?.Where(e => !string.IsNullOrWhiteSpace(e.EmailAddress)).ToList();
            vm.ContactPhones = vm.ContactPhones?.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList();
            vm.Addresses = vm.Addresses?.Where(a => !string.IsNullOrWhiteSpace(a.FullAddress)).ToList();

            ClearModelStateForList("Emails", vm.Emails?.Count ?? 0);
            ClearModelStateForList("ContactPhones", vm.ContactPhones?.Count ?? 0);
            ClearModelStateForList("Addresses", vm.Addresses?.Count ?? 0);

            if (!ModelState.IsValid)
            {
                await PopulateIndividuals(vm);
                return View(vm);
            }

            var company = new CustomerCompany
            {
                CompanyName = vm.CompanyName,
                EconomicCode = vm.EconomicCode,
                NationalId = vm.NationalId,
                RegisterNumber = vm.RegisterNumber,
                EstablishmentDate = vm.EstablishmentDate,
                IndustryField = vm.IndustryField,
                Website = vm.Website
            };

            _context.CustomerCompanies.Add(company);
            await _context.SaveChangesAsync();

            if (vm.SelectedIndividualCustomerId.HasValue && vm.SelectedIndividualCustomerId.Value > 0)
            {
                var relation = new CustomerCompanyRelation
                {
                    CompanyCustomerId = company.CustomerId,
                    IndividualCustomerId = vm.SelectedIndividualCustomerId.Value,
                    RelationType = vm.RelationType,
                    StartDate = vm.RelationStartDate,
                    Description = vm.RelationDescription
                };
                _context.CustomerCompanyRelations.Add(relation);
                await _context.SaveChangesAsync();
            }

            await AddEmailsAsync(company.CustomerId, vm.Emails);
            await AddPhonesAsync(company.CustomerId, vm.ContactPhones);
            await AddAddressesAsync(company.CustomerId, vm.Addresses);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var company = await _context.CustomerCompanies
                .Include(c => c.Emails)
                .Include(c => c.ContactPhones)
                .Include(c => c.Addresses)
                .Include(c => c.CustomerCompanyRelations)
                    .ThenInclude(r => r.IndividualCustomer)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (company == null) return NotFound();

            var individualCustomers = await _context.CustomerIndividuals
                .Select(ic => new IndividualCustomer
                {
                    CustomerId = ic.CustomerId,
                    FullName = $"{ic.FirstName} {ic.LastName}"
                }).ToListAsync();

            var emails = company.Emails?.Select(e => new EmailViewModel
            {
                EmailId = e.EmailId,
                EmailAddress = e.EmailAddress,
                EmailType = e.EmailType,
                IsPrimary = e.IsPrimary
            }).ToList() ?? new List<EmailViewModel>();

            var phones = company.ContactPhones?.Select(p => new PhoneViewModel
            {
                PhoneId = p.PhoneId,
                PhoneNumber = p.PhoneNumber,
                PhoneType = p.PhoneType,
                Extension = p.Extension
            }).ToList() ?? new List<PhoneViewModel>();

            var addresses = company.Addresses?.Select(a => new AddressViewModel
            {
                AddressId = a.AddressId,
                FullAddress = a.FullAddress,
                City = a.City,
                Province = a.Province,
                PostalCode = a.PostalCode,
                AddressType = a.AddressType
            }).ToList() ?? new List<AddressViewModel>();

            if (!emails.Any())
                emails.Add(new EmailViewModel());

            if (!phones.Any())
                phones.Add(new PhoneViewModel());

            if (!addresses.Any())
                addresses.Add(new AddressViewModel());

            var relations = company.CustomerCompanyRelations?.Select(r => new CustomerCompanyRelationViewModel
            {
                RelationId = r.RelationId,
                IndividualCustomerId = r.IndividualCustomerId,
                RelationType = r.RelationType,
                RelationStartDate = r.StartDate,
                RelationDescription = r.Description
            }).ToList() ?? new List<CustomerCompanyRelationViewModel>();

            // **اینجا مشکل حل شده: اگر روابط نال یا خالی بود، یک رکورد خالی اضافه کن**
            if (!relations.Any())
                relations.Add(new CustomerCompanyRelationViewModel());

            var vm = new CompanyEditViewModel
            {
                CustomerId = company.CustomerId,
                CompanyName = company.CompanyName,
                EconomicCode = company.EconomicCode,
                NationalId = company.NationalId,
                RegisterNumber = company.RegisterNumber,
                EstablishmentDate = company.EstablishmentDate,
                IndustryField = company.IndustryField,
                Website = company.Website,

                Emails = emails,
                ContactPhones = phones,
                Addresses = addresses,

                Relations = relations,
                IndividualCustomers = individualCustomers
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompanyEditViewModel vm)
        {
            if (id != vm.CustomerId) return NotFound();

            // اطمینان از مقداردهی لیست‌ها
            vm.Emails ??= new List<EmailViewModel>();
            vm.ContactPhones ??= new List<PhoneViewModel>();
            vm.Addresses ??= new List<AddressViewModel>();
            vm.Relations ??= new List<CustomerCompanyRelationViewModel>();

            // فقط موارد دارای مقدار معتبر نگهداری می‌شوند
            vm.Emails = vm.Emails.Where(e => !string.IsNullOrWhiteSpace(e.EmailAddress)).ToList();
            vm.ContactPhones = vm.ContactPhones.Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber)).ToList();
            vm.Addresses = vm.Addresses.Where(a => !string.IsNullOrWhiteSpace(a.FullAddress)).ToList();
            vm.Relations = vm.Relations.Where(r => r.IndividualCustomerId.HasValue).ToList();

            ClearModelStateForList("Emails", vm.Emails.Count);
            ClearModelStateForList("ContactPhones", vm.ContactPhones.Count);
            ClearModelStateForList("Addresses", vm.Addresses.Count);
            ClearModelStateForList("Relations", vm.Relations.Count);

            if (string.IsNullOrWhiteSpace(vm.CompanyName))
            {
                ModelState.AddModelError(nameof(vm.CompanyName), "نام شرکت باید وارد شود.");
            }

            if (!ModelState.IsValid)
                return View(vm);

            var company = await _context.CustomerCompanies
                .Include(c => c.Emails)
                .Include(c => c.ContactPhones)
                .Include(c => c.Addresses)
                .Include(c => c.CustomerCompanyRelations)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (company == null) return NotFound();

            company.CompanyName = vm.CompanyName;

            if (!string.IsNullOrWhiteSpace(vm.EconomicCode))
                company.EconomicCode = vm.EconomicCode;

            if (!string.IsNullOrWhiteSpace(vm.NationalId))
                company.NationalId = vm.NationalId;

            if (!string.IsNullOrWhiteSpace(vm.RegisterNumber))
                company.RegisterNumber = vm.RegisterNumber;

            if (vm.EstablishmentDate.HasValue)
                company.EstablishmentDate = vm.EstablishmentDate;

            if (!string.IsNullOrWhiteSpace(vm.IndustryField))
                company.IndustryField = vm.IndustryField;

            if (!string.IsNullOrWhiteSpace(vm.Website))
                company.Website = vm.Website;

            // حذف همه اطلاعات قبلی و جایگزینی
            _context.Emails.RemoveRange(company.Emails);
            _context.ContactPhones.RemoveRange(company.ContactPhones);
            _context.Addresses.RemoveRange(company.Addresses);
            _context.CustomerCompanyRelations.RemoveRange(company.CustomerCompanyRelations);

            await AddEmailsAsync(company.CustomerId, vm.Emails);
            await AddPhonesAsync(company.CustomerId, vm.ContactPhones);
            await AddAddressesAsync(company.CustomerId, vm.Addresses);

            foreach (var relationVm in vm.Relations)
            {
                if (relationVm.IndividualCustomerId.HasValue && relationVm.IndividualCustomerId.Value > 0)
                {
                    var relation = new CustomerCompanyRelation
                    {
                        CompanyCustomerId = company.CustomerId,
                        IndividualCustomerId = relationVm.IndividualCustomerId.Value,
                        RelationType = relationVm.RelationType,
                        StartDate = relationVm.RelationStartDate,
                        Description = relationVm.RelationDescription
                    };
                    _context.CustomerCompanyRelations.Add(relation);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int id)
        {
            var company = await _context.CustomerCompanies.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (company == null) return NotFound();

            _context.CustomerCompanies.Remove(company);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        private async Task PopulateIndividuals(CompanyCreateViewModel vm)
        {
            var individuals = await _context.CustomerIndividuals.ToListAsync();

            vm.IndividualCustomers = individuals.Any()
                ? individuals.Select(c => new IndividualCustomer
                {
                    CustomerId = c.CustomerId,
                    FullName = $"{c.FirstName} {c.LastName}"
                }).ToList()
                : new List<IndividualCustomer> { new IndividualCustomer { CustomerId = 0, FullName = "هیچ مشتری حقیقی موجود نیست" } };
        }

        private void ClearModelStateForList(string listName, int validCount)
        {
            for (int i = validCount; ; i++)
            {
                var prefix = $"{listName}[{i}]";
                var keys = ModelState.Keys.Where(k => k.StartsWith(prefix)).ToList();
                if (!keys.Any()) break;
                foreach (var key in keys)
                {
                    ModelState.Remove(key);
                }
            }
        }

        private void UpdateEmails(CustomerCompany company, List<EmailViewModel> emails)
        {
            // حذف ایمیل‌های حذف شده
            var toRemove = company.Emails.Where(e => !emails.Any(vm => vm.EmailId == e.EmailId)).ToList();
            _context.Emails.RemoveRange(toRemove);

            // اضافه یا ویرایش ایمیل‌ها
            foreach (var emailVm in emails)
            {
                var email = company.Emails.FirstOrDefault(e => e.EmailId == emailVm.EmailId);
                if (email == null)
                {
                    // اضافه جدید
                    company.Emails.Add(new Email
                    {
                        EmailAddress = emailVm.EmailAddress,
                        EmailType = emailVm.EmailType,
                        IsPrimary = emailVm.IsPrimary
                    });
                }
                else
                {
                    // ویرایش
                    email.EmailAddress = emailVm.EmailAddress;
                    email.EmailType = emailVm.EmailType;
                    email.IsPrimary = emailVm.IsPrimary;
                }
            }
        }

        private void UpdatePhones(CustomerCompany company, List<PhoneViewModel> phones)
        {
            var toRemove = company.ContactPhones.Where(p => !phones.Any(vm => vm.PhoneId == p.PhoneId)).ToList();
            _context.ContactPhones.RemoveRange(toRemove);

            foreach (var phoneVm in phones)
            {
                var phone = company.ContactPhones.FirstOrDefault(p => p.PhoneId == phoneVm.PhoneId);
                if (phone == null)
                {
                    company.ContactPhones.Add(new ContactPhone
                    {
                        PhoneNumber = phoneVm.PhoneNumber,
                        PhoneType = phoneVm.PhoneType,
                        Extension = phoneVm.Extension
                    });
                }
                else
                {
                    phone.PhoneNumber = phoneVm.PhoneNumber;
                    phone.PhoneType = phoneVm.PhoneType;
                    phone.Extension = phoneVm.Extension;
                }
            }
        }

        private void UpdateAddresses(CustomerCompany company, List<AddressViewModel> addresses)
        {
            var toRemove = company.Addresses.Where(a => !addresses.Any(vm => vm.AddressId == a.AddressId)).ToList();
            _context.Addresses.RemoveRange(toRemove);

            foreach (var addressVm in addresses)
            {
                var address = company.Addresses.FirstOrDefault(a => a.AddressId == addressVm.AddressId);
                if (address == null)
                {
                    company.Addresses.Add(new Address
                    {
                        FullAddress = addressVm.FullAddress,
                        City = addressVm.City,
                        Province = addressVm.Province,
                        PostalCode = addressVm.PostalCode,
                        AddressType = addressVm.AddressType
                    });
                }
                else
                {
                    address.FullAddress = addressVm.FullAddress;
                    address.City = addressVm.City;
                    address.Province = addressVm.Province;
                    address.PostalCode = addressVm.PostalCode;
                    address.AddressType = addressVm.AddressType;
                }
            }
        }

        private async Task AddEmailsAsync(int companyId, List<EmailViewModel> emails)
        {
            if (emails == null) return;
            foreach (var email in emails)
            {
                var entity = new Email
                {
                    CompanyCustomerId = companyId,
                    EmailAddress = email.EmailAddress,
                    EmailType = email.EmailType,
                    IsPrimary = email.IsPrimary
                };
                _context.Emails.Add(entity);
            }
            await _context.SaveChangesAsync();
        }

        private async Task AddPhonesAsync(int companyId, List<PhoneViewModel> phones)
        {
            if (phones == null) return;
            foreach (var phone in phones)
            {
                var entity = new ContactPhone
                {
                    CompanyCustomerId = companyId,
                    PhoneNumber = phone.PhoneNumber,
                    PhoneType = phone.PhoneType,
                    Extension = phone.Extension
                };
                _context.ContactPhones.Add(entity);
            }
            await _context.SaveChangesAsync();
        }

        private async Task AddAddressesAsync(int companyId, List<AddressViewModel> addresses)
        {
            if (addresses == null) return;
            foreach (var addr in addresses)
            {
                var entity = new Address
                {
                    CompanyCustomerId = companyId,
                    FullAddress = addr.FullAddress,
                    City = addr.City,
                    Province = addr.Province,
                    PostalCode = addr.PostalCode,
                    AddressType = addr.AddressType
                };
                _context.Addresses.Add(entity);
            }
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
