using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HomeBudgetAPI.Data;
using HomeBudgetAPI.Models;

namespace HomeBudgetAPI.Controllers
{
    [Authorize] // 🔐 protect all endpoints
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpenseController(AppDbContext context)
        {
            _context = context;
        }

        // 🔐 Get logged-in user email from token
        private string GetUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }

        // ✅ GET ALL (user-specific)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var email = GetUserEmail();

            var data = await _context.Expenses
                .Where(e => e.UserEmail == email)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(data);
        }

        // ✅ ADD
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Expense expense)
        {
            if (string.IsNullOrEmpty(expense.Title) || expense.Amount == 0)
                return BadRequest(new { message = "Invalid data" });

            // 🔐 attach user
            expense.UserEmail = GetUserEmail();
            expense.Date = DateTime.Now;

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return Ok(expense);
        }

        // ✏️ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Expense updated)
        {
            var email = GetUserEmail();

            var exp = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserEmail == email);

            if (exp == null) return NotFound();

            exp.Title = updated.Title;
            exp.Amount = updated.Amount;
            exp.Category = updated.Category;

            await _context.SaveChangesAsync();

            return Ok(exp);
        }

        // ❌ DELETE (you were missing this)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var email = GetUserEmail();

            var exp = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserEmail == email);

            if (exp == null) return NotFound();

            _context.Expenses.Remove(exp);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully" });
        }

        // 📊 SUMMARY (user-specific)
        [HttpGet("summary")]
        public async Task<IActionResult> Summary()
        {
            var email = GetUserEmail();

            var data = await _context.Expenses
                .Where(e => e.UserEmail == email)
                .GroupBy(e => new
                {
                    e.Category,
                    Type = e.Amount > 0 ? "Income" : "Expense"
                })
                .Select(g => new
                {
                    category = g.Key.Category,
                    type = g.Key.Type,
                    total = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}