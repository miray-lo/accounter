using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Data;
using AccountingSystem.Models;

namespace AccountingSystem.Forms
{
    public class AddEditForm : Form
    {
        // ── Controls ──────────────────────────────────────────────────────────────
        private DateTimePicker dtpDate;
        private RadioButton rbIncome, rbExpense;
        private ComboBox cboCategory;
        private NumericUpDown nudAmount;
        private TextBox txtNote;
        private Button btnSave, btnCancel;

        public Transaction Result { get; private set; }
        private readonly Transaction _editing;

        // ── Constructor ───────────────────────────────────────────────────────────
        public AddEditForm(Transaction editing = null)
        {
            _editing = editing;
            InitializeComponent();
            if (_editing != null) LoadData();
            else dtpDate.Value = DateTime.Today;
        }

        private void InitializeComponent()
        {
            Text = _editing == null ? "新增記錄" : "編輯記錄";
            Size = new Size(400, 340);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Microsoft JhengHei", 10f);

            // ── Labels & layout ───────────────────────────────────────────────────
            int lx = 30, cx = 130, lw = 90, cw = 220;
            int y = 25, gap = 40;

            var lblDate = new Label { Text = "日期", Location = new Point(lx, y+3), Size = new Size(lw, 22) };
            dtpDate = new DateTimePicker { Location = new Point(cx, y), Size = new Size(cw, 26),
                Format = DateTimePickerFormat.Short };
            y += gap;

            var lblType = new Label { Text = "類型", Location = new Point(lx, y+3), Size = new Size(lw, 22) };
            var pnlType = new Panel { Location = new Point(cx, y), Size = new Size(cw, 26) };
            rbExpense = new RadioButton { Text = "支出", Location = new Point(0, 3),  Size = new Size(70, 20), Checked = true };
            rbIncome  = new RadioButton { Text = "收入", Location = new Point(80, 3), Size = new Size(70, 20) };
            rbExpense.CheckedChanged += (s, e) => RefreshCategories();
            rbIncome.CheckedChanged  += (s, e) => RefreshCategories();
            pnlType.Controls.AddRange(new Control[] { rbExpense, rbIncome });
            y += gap;

            var lblCat = new Label { Text = "分類", Location = new Point(lx, y+3), Size = new Size(lw, 22) };
            cboCategory = new ComboBox { Location = new Point(cx, y), Size = new Size(cw, 26),
                DropDownStyle = ComboBoxStyle.DropDownList };
            y += gap;

            var lblAmt = new Label { Text = "金額 (NT$)", Location = new Point(lx, y+3), Size = new Size(lw, 22) };
            nudAmount = new NumericUpDown { Location = new Point(cx, y), Size = new Size(cw, 26),
                Minimum = 1, Maximum = 9_999_999, DecimalPlaces = 0, ThousandsSeparator = true };
            y += gap;

            var lblNote = new Label { Text = "備註", Location = new Point(lx, y+3), Size = new Size(lw, 22) };
            txtNote = new TextBox { Location = new Point(cx, y), Size = new Size(cw, 26),
                MaxLength = 100 };
            y += gap + 10;

            // ── Buttons ───────────────────────────────────────────────────────────
            btnSave = new Button
            {
                Text = "儲存", Size = new Size(100, 34),
                Location = new Point(cx, y),
                BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Microsoft JhengHei", 10f, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "取消", Size = new Size(100, 34),
                Location = new Point(cx + 110, y),
                BackColor = Color.FromArgb(189, 195, 199), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[]
            {
                lblDate, dtpDate,
                lblType, pnlType,
                lblCat, cboCategory,
                lblAmt, nudAmount,
                lblNote, txtNote,
                btnSave, btnCancel
            });

            RefreshCategories();
        }

        private void RefreshCategories()
        {
            string type = rbIncome.Checked ? "收入" : "支出";
            cboCategory.Items.Clear();
            foreach (var c in DatabaseHelper.GetCategories(type))
                cboCategory.Items.Add(c);
            if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
        }

        private void LoadData()
        {
            dtpDate.Value = _editing.Date;
            if (_editing.Type == "收入") rbIncome.Checked = true;
            else rbExpense.Checked = true;
            RefreshCategories();
            cboCategory.Text = _editing.Category;
            nudAmount.Value = _editing.Amount;
            txtNote.Text = _editing.Note;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cboCategory.SelectedIndex < 0)
            { MessageBox.Show("請選擇分類", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            Result = new Transaction
            {
                Id       = _editing?.Id ?? 0,
                Date     = dtpDate.Value,
                Type     = rbIncome.Checked ? "收入" : "支出",
                Category = cboCategory.Text,
                Amount   = nudAmount.Value,
                Note     = txtNote.Text.Trim()
            };
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
