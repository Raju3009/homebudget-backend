using Microsoft.AspNetCore.Mvc;
using HomeBudgetAPI.Data;
using HomeBudgetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;

namespace HomeBudgetAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpenseController(AppDbContext context)
        {
            _context = context;
        }

        // 🔑 SAFE USER ID METHOD (FIXED WARNING)
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                throw new UnauthorizedAccessException("Invalid token");

            return int.Parse(userIdClaim.Value);
        }

        // ➕ ADD EXPENSE
        [HttpPost]
        public IActionResult AddExpense(Expense expense)
        {
            var userId = GetUserId();

            expense.UserId = userId;
            expense.Date = expense.Date == default ? DateTime.Now : expense.Date;

            _context.Expenses.Add(expense);
            _context.SaveChanges();

            return Ok(new { message = "Expense added ✅" });
        }

        // 📊 GET ALL (USER-SPECIFIC)
        [HttpGet]
        public IActionResult GetExpenses()
        {
            var userId = GetUserId();

            var data = _context.Expenses
                .Where(e => e.UserId == userId)
                .ToList();

            return Ok(data);
        }

        // 💰 TOTAL
        [HttpGet("total")]
        public IActionResult GetTotalExpense()
        {
            var userId = GetUserId();

            var total = _context.Expenses
                .Where(e => e.UserId == userId)
                .Sum(e => e.Amount);

            return Ok(new { total });
        }

        // 📅 MONTHLY SUMMARY
        [HttpGet("monthly")]
        public IActionResult GetMonthlySummary()
        {
            var userId = GetUserId();

            var data = _context.Expenses
                .Where(e => e.UserId == userId)
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new
                {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    total = g.Sum(x => x.Amount)
                })
                .ToList();

            return Ok(data);
        }

        // 🤖 INSIGHTS
        [HttpGet("insights")]
        public IActionResult GetInsights()
        {
            var userId = GetUserId();

            var total = _context.Expenses
                .Where(e => e.UserId == userId)
                .Sum(e => e.Amount);

            if (total == 0)
                return Ok(new[] { "No data available" });

            var insights = new System.Collections.Generic.List<string>();

            if (total > 5000)
                insights.Add("⚠️ Your spending is high this month");

            if (total < 2000)
                insights.Add("✅ Your spending is under control");

            if (!insights.Any())
                insights.Add("📊 Keep tracking your expenses");

            return Ok(insights);
        }

        // ✏️ UPDATE
        [HttpPut("{id}")]
        public IActionResult UpdateExpense(int id, Expense updatedExpense)
        {
            var userId = GetUserId();

            var expense = _context.Expenses
                .FirstOrDefault(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound(new { message = "Expense not found ❌" });

            expense.Title = updatedExpense.Title;
            expense.Amount = updatedExpense.Amount;
            expense.Date = updatedExpense.Date;

            _context.SaveChanges();

            return Ok(new { message = "Expense updated ✅" });
        }

        // 🗑️ DELETE
        [HttpDelete("{id}")]
        public IActionResult DeleteExpense(int id)
        {
            var userId = GetUserId();

            var expense = _context.Expenses
                .FirstOrDefault(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound(new { message = "Expense not found ❌" });

            _context.Expenses.Remove(expense);
            _context.SaveChanges();

            return Ok(new { message = "Expense deleted ✅" });
        }
    }
}