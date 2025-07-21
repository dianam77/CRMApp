using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMApp.Data;
using CRMApp.Models;
using CRMApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CRMApp.Controllers
{
    // به طور کلی فقط Admin و Manager اجازه دسترسی به اکشن‌های غیر از Index دارند
    [Authorize(Roles = "Admin,Manager")]
    public class CustomerIndividualController : Controller
    {
        private readonly CRMAppDbContext _context;

        public CustomerIndividualController(CRMAppDbContext context)
        {
            _context = context;
        }

        // فقط این اکشن باز است برای User هم
        [AllowAnonymous]
        [Authorize(Roles = "Admin,Manager,User")]
        // GET: /CustomerIndividual/Index
        public async Task<IActionResult> Index()
        {
            var individuals = await _context.CustomerIndividuals
                .Include(i => i.Emails)
                .Include(i => i.ContactPhones)
                .Include(i => i.Addresses)
                .ToListAsync();

            return View(individuals);
        }

        // GET: /CustomerIndividual/Create
        public IActionResult Create()
        {
            var vm = new CustomerIndividualViewModel
            {
                Emails = new System.Collections.Generic.List<EmailViewModel> { new EmailViewModel() },
                ContactPhones = new System.Collections.Generic.List<PhoneViewModel> { new PhoneViewModel() },
                Addresses = new System.Collections.Generic.List<AddressViewModel> { new AddressViewModel() }
            };
            return View(vm);
        }

        // POST: /CustomerIndividual/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerIndividualViewModel vm)
        {
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

        // GET: /CustomerIndividual/Edit/5
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
                Emails = individual.Emails.Select(e => new EmailViewModel
                {
                    EmailId = e.EmailId,
                    EmailAddress = e.EmailAddress,
                    EmailType = e.EmailType,
                    IsPrimary = e.IsPrimary
                }).ToList(),
                ContactPhones = individual.ContactPhones.Select(p => new PhoneViewModel
                {
                    PhoneId = p.PhoneId,
                    PhoneNumber = p.PhoneNumber,
                    PhoneType = p.PhoneType,
                    Extension = p.Extension
                }).ToList(),
                Addresses = individual.Addresses.Select(a => new AddressViewModel
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

        // POST: /CustomerIndividual/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerIndividualViewModel vm)
        {
            if (id != vm.CustomerId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(vm);

            var existing = await _context.CustomerIndividuals
                .Include(i => i.Emails)
                .Include(i => i.ContactPhones)
                .Include(i => i.Addresses)
                .FirstOrDefaultAsync(i => i.CustomerId == id);

            if (existing == null)
                return NotFound();

            existing.FirstName = vm.FirstName;
            existing.LastName = vm.LastName;
            existing.FatherName = vm.FatherName;
            existing.BirthDate = vm.BirthDate;
            existing.NationalCode = vm.NationalCode;
            existing.IdentityNumber = vm.IdentityNumber;
            existing.Gender = vm.Gender;
            existing.MaritalStatus = vm.MaritalStatus;

            UpdateEmails(existing, vm.Emails);
            UpdatePhones(existing, vm.ContactPhones);
            UpdateAddresses(existing, vm.Addresses);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /CustomerIndividual/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var individual = await _context.CustomerIndividuals.FindAsync(id);
            if (individual == null)
                return NotFound();

            return View(individual);
        }

        // POST: /CustomerIndividual/Delete/5
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

        private async Task AddEmailsAsync(int individualId, System.Collections.Generic.List<EmailViewModel> emails)
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

        private async Task AddPhonesAsync(int individualId, System.Collections.Generic.List<PhoneViewModel> phones)
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

        private async Task AddAddressesAsync(int individualId, System.Collections.Generic.List<AddressViewModel> addresses)
        {
            var validAddresses = addresses?
                .Where(a => !string.IsNullOrWhiteSpace(a.FullAddress))
                .Select(a => new Address
                {
                    IndividualCustomerId = individualId,
                    FullAddress = a.FullAddress,
                    City = a.City,
                    Province = a.Province,
                    PostalCode = a.PostalCode,
                    AddressType = a.AddressType
                }).ToList();

            if (validAddresses?.Any() == true)
            {
                _context.Addresses.AddRange(validAddresses);
                await _context.SaveChangesAsync();
            }
        }

        private void UpdateEmails(CustomerIndividual individual, System.Collections.Generic.List<EmailViewModel> updatedEmails)
        {
            if (updatedEmails == null) return;

            var emailsToRemove = individual.Emails.Where(e => !updatedEmails.Any(u => u.EmailId == e.EmailId)).ToList();
            _context.Emails.RemoveRange(emailsToRemove);

            foreach (var email in updatedEmails)
            {
                var existingEmail = individual.Emails.FirstOrDefault(e => e.EmailId == email.EmailId);
                if (existingEmail != null)
                {
                    existingEmail.EmailAddress = email.EmailAddress;
                    existingEmail.EmailType = email.EmailType;
                    existingEmail.IsPrimary = email.IsPrimary;
                }
                else
                {
                    individual.Emails.Add(new Email
                    {
                        EmailAddress = email.EmailAddress,
                        EmailType = email.EmailType,
                        IsPrimary = email.IsPrimary
                    });
                }
            }
        }

        private void UpdatePhones(CustomerIndividual individual, System.Collections.Generic.List<PhoneViewModel> updatedPhones)
        {
            if (updatedPhones == null) return;

            var phonesToRemove = individual.ContactPhones.Where(p => !updatedPhones.Any(u => u.PhoneId == p.PhoneId)).ToList();
            _context.ContactPhones.RemoveRange(phonesToRemove);

            foreach (var phone in updatedPhones)
            {
                var existingPhone = individual.ContactPhones.FirstOrDefault(p => p.PhoneId == phone.PhoneId);
                if (existingPhone != null)
                {
                    existingPhone.PhoneNumber = phone.PhoneNumber;
                    existingPhone.PhoneType = phone.PhoneType;
                    existingPhone.Extension = phone.Extension;
                }
                else
                {
                    individual.ContactPhones.Add(new ContactPhone
                    {
                        PhoneNumber = phone.PhoneNumber,
                        PhoneType = phone.PhoneType,
                        Extension = phone.Extension
                    });
                }
            }
        }

        private void UpdateAddresses(CustomerIndividual individual, System.Collections.Generic.List<AddressViewModel> updatedAddresses)
        {
            if (updatedAddresses == null) return;

            var addressesToRemove = individual.Addresses.Where(a => !updatedAddresses.Any(u => u.AddressId == a.AddressId)).ToList();
            _context.Addresses.RemoveRange(addressesToRemove);

            foreach (var address in updatedAddresses)
            {
                var existingAddress = individual.Addresses.FirstOrDefault(a => a.AddressId == address.AddressId);
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
                    individual.Addresses.Add(new Address
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
