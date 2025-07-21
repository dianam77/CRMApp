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

            await AddRelationAsync(company.CustomerId, vm);
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
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (company == null) return NotFound();

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
                Emails = company.Emails.Select(e => new EmailViewModel
                {
                    EmailId = e.EmailId,
                    EmailAddress = e.EmailAddress,
                    EmailType = e.EmailType,
                    IsPrimary = e.IsPrimary
                }).ToList(),
                ContactPhones = company.ContactPhones.Select(p => new PhoneViewModel
                {
                    PhoneId = p.PhoneId,
                    PhoneNumber = p.PhoneNumber,
                    PhoneType = p.PhoneType,
                    Extension = p.Extension
                }).ToList(),
                Addresses = company.Addresses.Select(a => new AddressViewModel
                {
                    AddressId = a.AddressId,
                    FullAddress = a.FullAddress,
                    City = a.City,
                    Province = a.Province,
                    PostalCode = a.PostalCode,
                    AddressType = a.AddressType
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompanyEditViewModel vm)
        {
            if (id != vm.CustomerId) return NotFound();

            if (!ModelState.IsValid)
                return View(vm);

            var company = await _context.CustomerCompanies
                .Include(c => c.Emails)
                .Include(c => c.ContactPhones)
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (company == null) return NotFound();

            company.CompanyName = vm.CompanyName;
            company.EconomicCode = vm.EconomicCode;
            company.NationalId = vm.NationalId;
            company.RegisterNumber = vm.RegisterNumber;
            company.EstablishmentDate = vm.EstablishmentDate;
            company.IndustryField = vm.IndustryField;
            company.Website = vm.Website;

            UpdateEmails(company, vm.Emails);
            UpdatePhones(company, vm.ContactPhones);
            UpdateAddresses(company, vm.Addresses);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var company = await _context.CustomerCompanies.FirstOrDefaultAsync(c => c.CustomerId == id);
            return company == null ? NotFound() : View(company);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.CustomerCompanies
                .Include(c => c.Emails)
                .Include(c => c.ContactPhones)
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (company != null)
            {
                _context.Emails.RemoveRange(company.Emails);
                _context.ContactPhones.RemoveRange(company.ContactPhones);
                _context.Addresses.RemoveRange(company.Addresses);
                _context.CustomerCompanies.Remove(company);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id) =>
            _context.CustomerCompanies.Any(e => e.CustomerId == id);

        private async Task PopulateIndividuals(CompanyCreateViewModel vm)
        {
            vm.IndividualCustomers = await _context.CustomerIndividuals
                .Select(c => new IndividualCustomer
                {
                    CustomerId = c.CustomerId,
                    FullName = $"{c.FirstName} {c.LastName}"
                }).ToListAsync();
        }

        private async Task AddRelationAsync(int companyId, CompanyCreateViewModel vm)
        {
            if (vm.SelectedIndividualCustomerId.GetValueOrDefault() > 0)
            {
                var relation = new CustomerCompanyRelation
                {
                    CompanyCustomerId = companyId,
                    IndividualCustomerId = vm.SelectedIndividualCustomerId.Value,
                    RelationType = vm.RelationType,
                    StartDate = vm.RelationStartDate,
                    Description = vm.RelationDescription
                };
                _context.CustomerCompanyRelations.Add(relation);
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddEmailsAsync(int companyId, List<EmailViewModel> emails)
        {
            if (emails == null) return;

            var validEmails = emails
                .Where(e => !string.IsNullOrWhiteSpace(e.EmailAddress))
                .Select(e => new Email
                {
                    CompanyCustomerId = companyId,
                    EmailAddress = e.EmailAddress,
                    EmailType = e.EmailType,
                    IsPrimary = e.IsPrimary
                }).ToList();

            if (validEmails.Any())
            {
                _context.Emails.AddRange(validEmails);
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddPhonesAsync(int companyId, List<PhoneViewModel> phones)
        {
            if (phones == null) return;

            var validPhones = phones
                .Where(p => !string.IsNullOrWhiteSpace(p.PhoneNumber))
                .Select(p => new ContactPhone
                {
                    CompanyCustomerId = companyId,
                    PhoneNumber = p.PhoneNumber,
                    PhoneType = p.PhoneType,
                    Extension = p.Extension
                }).ToList();

            if (validPhones.Any())
            {
                _context.ContactPhones.AddRange(validPhones);
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddAddressesAsync(int companyId, List<AddressViewModel> addresses)
        {
            if (addresses == null) return;

            var validAddresses = addresses
                .Where(a => !string.IsNullOrWhiteSpace(a.FullAddress))
                .Select(a => new Address
                {
                    CompanyCustomerId = companyId,
                    FullAddress = a.FullAddress,
                    City = a.City,
                    Province = a.Province,
                    PostalCode = a.PostalCode,
                    AddressType = a.AddressType
                }).ToList();

            if (validAddresses.Any())
            {
                _context.Addresses.AddRange(validAddresses);
                await _context.SaveChangesAsync();
            }
        }

        private void UpdateEmails(CustomerCompany company, List<EmailViewModel> updatedEmails)
        {
            if (updatedEmails == null) return;

            var emailsToRemove = company.Emails.Where(e => updatedEmails.All(u => u.EmailId != e.EmailId)).ToList();
            _context.Emails.RemoveRange(emailsToRemove);

            foreach (var email in updatedEmails)
            {
                var existingEmail = company.Emails.FirstOrDefault(e => e.EmailId == email.EmailId);
                if (existingEmail != null)
                {
                    existingEmail.EmailAddress = email.EmailAddress;
                    existingEmail.EmailType = email.EmailType;
                    existingEmail.IsPrimary = email.IsPrimary;
                }
                else
                {
                    company.Emails.Add(new Email
                    {
                        EmailAddress = email.EmailAddress,
                        EmailType = email.EmailType,
                        IsPrimary = email.IsPrimary
                    });
                }
            }
        }

        private void UpdatePhones(CustomerCompany company, List<PhoneViewModel> updatedPhones)
        {
            if (updatedPhones == null) return;

            var phonesToRemove = company.ContactPhones.Where(p => updatedPhones.All(u => u.PhoneId != p.PhoneId)).ToList();
            _context.ContactPhones.RemoveRange(phonesToRemove);

            foreach (var phone in updatedPhones)
            {
                var existingPhone = company.ContactPhones.FirstOrDefault(p => p.PhoneId == phone.PhoneId);
                if (existingPhone != null)
                {
                    existingPhone.PhoneNumber = phone.PhoneNumber;
                    existingPhone.PhoneType = phone.PhoneType;
                    existingPhone.Extension = phone.Extension;
                }
                else
                {
                    company.ContactPhones.Add(new ContactPhone
                    {
                        PhoneNumber = phone.PhoneNumber,
                        PhoneType = phone.PhoneType,
                        Extension = phone.Extension
                    });
                }
            }
        }

        private void UpdateAddresses(CustomerCompany company, List<AddressViewModel> updatedAddresses)
        {
            if (updatedAddresses == null) return;

            var addressesToRemove = company.Addresses.Where(a => updatedAddresses.All(u => u.AddressId != a.AddressId)).ToList();
            _context.Addresses.RemoveRange(addressesToRemove);

            foreach (var address in updatedAddresses)
            {
                var existingAddress = company.Addresses.FirstOrDefault(a => a.AddressId == address.AddressId);
                if (existingAddress != null)
                {
                    existingAddress.FullAddress = address.FullAddress;
                    existingAddress.City = address.City;
                    existingAddress.Province = address.Province;
                    existingAddress.PostalCode = address.PostalCode;
                    existingAddress.AddressType = address.AddressType;
                }
                else
                {
                    company.Addresses.Add(new Address
                    {
                        FullAddress = address.FullAddress,
                        City = address.City,
                        Province = address.Province,
                        PostalCode = address.PostalCode,
                        AddressType = address.AddressType
                    });
                }
            }
        }
    }
}
