public class Expense
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int UserId { get; set; }  // 🔐 User-based data

    public DateTime Date { get; set; } = DateTime.Now; // 📅 Date tracking
}