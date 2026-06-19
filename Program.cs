using System;
using System.Windows.Forms;
using AccountingSystem.Data;
using AccountingSystem.Forms;

namespace AccountingSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DatabaseHelper.Initialize();
            Application.Run(new MainForm());
        }
    }
}
