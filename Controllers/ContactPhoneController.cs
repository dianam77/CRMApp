using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRMApp.Models;
using CRMApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace CRMApp.Controllers
{
    [Authorize(Roles = "Admin,ContactPhoneManager")]
    public class ContactPhoneController : Controller
    {
        private readonly CRMAppDbContext _context;

        public ContactPhoneController(CRMAppDbContext context)
        {
            _context = context;
        }

        // GET: /ContactPhone
        public async Task<IActionResult> Index()
        {
            var phones = await _context.ContactPhones.ToListAsync();
            return View(phones);
        }

        // GET: /ContactPhone/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /ContactPhone/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContactPhone phone)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phone);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(phone);
        }

        // GET: /ContactPhone/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phone = await _context.ContactPhones.FindAsync(id);
            if (phone == null) return NotFound();

            return View(phone);
        }

        // POST: /ContactPhone/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContactPhone phone)
        {
            if (id != phone.PhoneId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phone);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ContactPhones.Any(e => e.PhoneId == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(phone);
        }

        // GET: /ContactPhone/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var phone = await _context.ContactPhones.FindAsync(id);
            if (phone == null) return NotFound();

            return View(phone);
        }

        // POST: /ContactPhone/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phone = await _context.ContactPhones.FindAsync(id);
            if (phone != null)
            {
                _context.ContactPhones.Remove(phone);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
