using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using AccountingSystem.Models;

namespace AccountingSystem.Data
{
    public class DatabaseHelper
    {
        private static readonly string DbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "accounting.db");
        private static readonly string ConnStr = $"Data Source={DbPath};Version=3;";

        public static void Initialize()
        {
            if (!File.Exists(DbPath))
                SQLiteConnection.CreateFile(DbPath);

            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                string sql = @"
                    CREATE TABLE IF NOT EXISTS Transactions (
                        Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                        Date     TEXT    NOT NULL,
                        Type     TEXT    NOT NULL,
                        Category TEXT    NOT NULL,
                        Amount   REAL    NOT NULL,
                        Note     TEXT
                    );
                    CREATE TABLE IF NOT EXISTS Categories (
                        Id   INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Type TEXT NOT NULL
                    );";
                new SQLiteCommand(sql, conn).ExecuteNonQuery();
                SeedCategories(conn);
            }
        }

        private static void SeedCategories(SQLiteConnection conn)
        {
            string check = "SELECT COUNT(*) FROM Categories";
            long count = (long)new SQLiteCommand(check, conn).ExecuteScalar();
            if (count > 0) return;

            string insert = "INSERT INTO Categories (Name, Type) VALUES (@n, @t)";
            var defaults = new[]
            {
                ("餐飲", "支出"), ("交通", "支出"), ("娛樂", "支出"),
                ("購物", "支出"), ("醫療", "支出"), ("住宿", "支出"), ("其他支出", "支出"),
                ("薪水", "收入"), ("兼職", "收入"), ("投資", "收入"), ("其他收入", "收入")
            };
            foreach (var (name, type) in defaults)
            {
                var cmd = new SQLiteCommand(insert, conn);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@t", type);
                cmd.ExecuteNonQuery();
            }
        }

        // ── Transactions ──────────────────────────────────────────────────────────

        public static List<Transaction> GetAll(int? year = null, int? month = null, string type = null)
        {
            var list = new List<Transaction>();
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                string sql = "SELECT * FROM Transactions WHERE 1=1";
                if (year.HasValue)  sql += " AND strftime('%Y', Date) = @y";
                if (month.HasValue) sql += " AND strftime('%m', Date) = @m";
                if (!string.IsNullOrEmpty(type)) sql += " AND Type = @t";
                sql += " ORDER BY Date DESC";

                var cmd = new SQLiteCommand(sql, conn);
                if (year.HasValue)  cmd.Parameters.AddWithValue("@y", year.Value.ToString());
                if (month.HasValue) cmd.Parameters.AddWithValue("@m", month.Value.ToString("D2"));
                if (!string.IsNullOrEmpty(type)) cmd.Parameters.AddWithValue("@t", type);

                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(Map(r));
            }
            return list;
        }

        public static void Insert(Transaction t)
        {
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                string sql = @"INSERT INTO Transactions (Date, Type, Category, Amount, Note)
                               VALUES (@d,@tp,@c,@a,@n)";
                var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@d",  t.Date.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@tp", t.Type);
                cmd.Parameters.AddWithValue("@c",  t.Category);
                cmd.Parameters.AddWithValue("@a",  t.Amount);
                cmd.Parameters.AddWithValue("@n",  t.Note ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update(Transaction t)
        {
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                string sql = @"UPDATE Transactions
                               SET Date=@d, Type=@tp, Category=@c, Amount=@a, Note=@n
                               WHERE Id=@id";
                var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@d",  t.Date.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@tp", t.Type);
                cmd.Parameters.AddWithValue("@c",  t.Category);
                cmd.Parameters.AddWithValue("@a",  t.Amount);
                cmd.Parameters.AddWithValue("@n",  t.Note ?? "");
                cmd.Parameters.AddWithValue("@id", t.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id)
        {
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                var cmd = new SQLiteCommand("DELETE FROM Transactions WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ── Summary ───────────────────────────────────────────────────────────────

        public static (decimal income, decimal expense) GetMonthSummary(int year, int month)
        {
            decimal income = 0, expense = 0;
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                string sql = @"SELECT Type, SUM(Amount) FROM Transactions
                               WHERE strftime('%Y', Date)=@y AND strftime('%m', Date)=@m
                               GROUP BY Type";
                var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@y", year.ToString());
                cmd.Parameters.AddWithValue("@m", month.ToString("D2"));
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        if (r.GetString(0) == "收入") income  = r.GetDecimal(1);
                        else                           expense = r.GetDecimal(1);
                    }
            }
            return (income, expense);
        }

        public static Dictionary<string, decimal> GetCategoryTotals(int year, int month, string type)
        {
            var dict = new Dictionary<string, decimal>();
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                string sql = @"SELECT Category, SUM(Amount) FROM Transactions
                               WHERE strftime('%Y', Date)=@y AND strftime('%m', Date)=@m
                                 AND Type=@t
                               GROUP BY Category";
                var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@y", year.ToString());
                cmd.Parameters.AddWithValue("@m", month.ToString("D2"));
                cmd.Parameters.AddWithValue("@t", type);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        dict[r.GetString(0)] = r.GetDecimal(1);
            }
            return dict;
        }

        // ── Categories ────────────────────────────────────────────────────────────

        public static List<string> GetCategories(string type)
        {
            var list = new List<string>();
            using (var conn = new SQLiteConnection(ConnStr))
            {
                conn.Open();
                var cmd = new SQLiteCommand(
                    "SELECT Name FROM Categories WHERE Type=@t ORDER BY Id", conn);
                cmd.Parameters.AddWithValue("@t", type);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(r.GetString(0));
            }
            return list;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static Transaction Map(SQLiteDataReader r) => new Transaction
        {
            Id       = r.GetInt32(0),
            Date     = DateTime.Parse(r.GetString(1)),
            Type     = r.GetString(2),
            Category = r.GetString(3),
            Amount   = r.GetDecimal(4),
            Note     = r.IsDBNull(5) ? "" : r.GetString(5)
        };
    }
}
