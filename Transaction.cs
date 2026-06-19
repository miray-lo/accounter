using System;

namespace AccountingSystem.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }       // "收入" or "支出"
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }

        public override string ToString()
        {
            return $"{Date:yyyy/MM/dd} [{Type}] {Category} ${Amount:N0} {Note}";
        }
    }
}
