using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var transactions = await _context.Transactions.Include(t => t.Category).ToListAsync();
            return View(transactions);
        }

        // GET: Transaction/AddOrEdit
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            await PopulateCategoriesAsync();
            if (id == 0)
                return View(new Transaction());

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound();

            return View(transaction);
        }

        // POST: Transaction/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,CategoryId,Amount,Note,Date")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                if (transaction.TransactionId == 0)
                    _context.Add(transaction);
                else
                    _context.Update(transaction);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    // Log exception and return error message
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            await PopulateCategoriesAsync();
            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            try
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Log exception and return error message
                ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        [NonAction]
        public async Task PopulateCategoriesAsync()
        {
            var categoryCollection = await _context.Categories.ToListAsync();
            var defaultCategory = new Category { CategoryId = 0, Title = "Choose a Category" };
            categoryCollection.Insert(0, defaultCategory);
            ViewBag.Categories = categoryCollection;
        }
    }
}
