using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Data;
using AccountingSystem.Models;

namespace AccountingSystem.Forms
{
    public class MainForm : Form
    {
        // ── Summary cards ─────────────────────────────────────────────────────────
        private Label lblIncomeVal, lblExpenseVal, lblBalanceVal;
        private Label lblMonthTitle;

        // ── Filter bar ────────────────────────────────────────────────────────────
        private ComboBox cboYear, cboMonth, cboTypeFilter;
        private Button btnSearch;

        // ── Grid ──────────────────────────────────────────────────────────────────
        private DataGridView dgv;

        // ── Toolbar ───────────────────────────────────────────────────────────────
        private Button btnAdd, btnEdit, btnDelete, btnChart;

        // ── Status bar ────────────────────────────────────────────────────────────
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;

        public MainForm()
        {
            InitializeComponent();
            LoadData();
        }

        // ─────────────────────────────────────────────────────────────────────────
        private void InitializeComponent()
        {
            Text = "個人記帳系統";
            Size = new Size(950, 660);
            MinimumSize = new Size(800, 580);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(236, 240, 241);
            Font = new Font("Microsoft JhengHei", 10f);

            // ── Header ────────────────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 60,
                BackColor = Color.FromArgb(44, 62, 80)
            };
            var lblTitle = new Label
            {
                Text = "💰 個人記帳系統",
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 16f, FontStyle.Bold),
                AutoSize = true, Location = new Point(20, 14)
            };
            pnlHeader.Controls.Add(lblTitle);

            // ── Summary cards ─────────────────────────────────────────────────────
            var pnlSummary = new Panel
            {
                Dock = DockStyle.Top, Height = 110,
                BackColor = Color.FromArgb(236, 240, 241), Padding = new Padding(10, 8, 10, 0)
            };
            lblMonthTitle = new Label
            {
                Text = $"{DateTime.Today:yyyy年MM月} 摘要",
                Font = new Font("Microsoft JhengHei", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Location = new Point(20, 5), AutoSize = true
            };
            pnlSummary.Controls.Add(lblMonthTitle);

            var cardIncome  = MakeCard("本月收入", Color.FromArgb(39, 174, 96),  out lblIncomeVal);
            var cardExpense = MakeCard("本月支出", Color.FromArgb(231, 76, 60),  out lblExpenseVal);
            var cardBalance = MakeCard("本月結餘", Color.FromArgb(52, 152, 219), out lblBalanceVal);

            // Anchor cards in a flow panel
            var flowCards = new FlowLayoutPanel
            {
                Location = new Point(10, 28), Size = new Size(900, 75),
                FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            flowCards.Controls.AddRange(new Control[] { cardIncome, cardExpense, cardBalance });
            pnlSummary.Controls.Add(flowCards);

            // ── Filter bar ────────────────────────────────────────────────────────
            var pnlFilter = new Panel
            {
                Dock = DockStyle.Top, Height = 48,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            int fx = 12;
            AddLabel(pnlFilter, "年份", fx, 14); fx += 46;
            cboYear = AddCombo(pnlFilter, fx, 10, 80); fx += 88;
            AddLabel(pnlFilter, "月份", fx, 14); fx += 46;
            cboMonth = AddCombo(pnlFilter, fx, 10, 80); fx += 88;
            AddLabel(pnlFilter, "類型", fx, 14); fx += 46;
            cboTypeFilter = AddCombo(pnlFilter, fx, 10, 90); fx += 98;

            for (int y = DateTime.Today.Year; y >= DateTime.Today.Year - 5; y--)
                cboYear.Items.Add(y);
            cboYear.Items.Insert(0, "全部"); cboYear.SelectedIndex = 1;   // current year

            cboMonth.Items.Add("全部");
            for (int m = 1; m <= 12; m++) cboMonth.Items.Add(m + " 月");
            cboMonth.SelectedIndex = DateTime.Today.Month;               // current month

            cboTypeFilter.Items.AddRange(new object[] { "全部", "收入", "支出" });
            cboTypeFilter.SelectedIndex = 0;

            btnSearch = MakeButton("🔍 查詢", fx, 10, 90, Color.FromArgb(52, 152, 219));
            btnSearch.Click += (s, e) => LoadData();
            pnlFilter.Controls.Add(btnSearch);

            // ── Toolbar ───────────────────────────────────────────────────────────
            var pnlToolbar = new Panel
            {
                Dock = DockStyle.Top, Height = 48,
                BackColor = Color.White
            };
            btnAdd    = MakeButton("➕ 新增", 12,  9, 90, Color.FromArgb(39, 174, 96));
            btnEdit   = MakeButton("✏️ 編輯", 110, 9, 90, Color.FromArgb(243, 156, 18));
            btnDelete = MakeButton("🗑️ 刪除", 208, 9, 90, Color.FromArgb(231, 76, 60));
            btnChart  = MakeButton("📊 圖表", 340, 9, 90, Color.FromArgb(155, 89, 182));
            btnAdd.Click    += BtnAdd_Click;
            btnEdit.Click   += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnChart.Click  += (s, e) => new ChartForm().ShowDialog();
            pnlToolbar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnChart });

            // ── DataGridView ──────────────────────────────────────────────────────
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                Font = new Font("Microsoft JhengHei", 10f),
                GridColor = Color.FromArgb(220, 220, 220),
                ColumnHeadersHeight = 36
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 62, 80);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Microsoft JhengHei", 10f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(173, 216, 230);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.RowTemplate.Height = 32;
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            // ── Status bar ────────────────────────────────────────────────────────
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("就緒") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            statusStrip.Items.Add(lblStatus);

            // ── Add to form (order matters for Dock) ──────────────────────────────
            Controls.Add(dgv);
            Controls.Add(pnlFilter);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlSummary);
            Controls.Add(pnlHeader);
            Controls.Add(statusStrip);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private Panel MakeCard(string title, Color color, out Label valueLabel)
        {
            var card = new Panel
            {
                Size = new Size(200, 66), BackColor = color, Margin = new Padding(0, 0, 12, 0)
            };
            var lTitle = new Label
            {
                Text = title, ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Microsoft JhengHei", 9f),
                Location = new Point(12, 8), AutoSize = true
            };
            valueLabel = new Label
            {
                Text = "$0", ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 18f, FontStyle.Bold),
                Location = new Point(10, 26), AutoSize = true
            };
            card.Controls.AddRange(new Control[] { lTitle, valueLabel });
            return card;
        }

        private static Button MakeButton(string text, int x, int y, int w, Color color)
        {
            var btn = new Button
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, 30),
                BackColor = color, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Microsoft JhengHei", 9.5f)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text, Location = new Point(x, y), AutoSize = true,
                ForeColor = Color.FromArgb(80, 80, 80)
            });
        }

        private static ComboBox AddCombo(Control parent, int x, int y, int w)
        {
            var c = new ComboBox
            {
                Location = new Point(x, y), Size = new Size(w, 26),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            parent.Controls.Add(c);
            return c;
        }

        // ── Data ──────────────────────────────────────────────────────────────────
        private void LoadData()
        {
            int? year  = cboYear.SelectedIndex  == 0 ? (int?)null : (int)cboYear.SelectedItem;
            int? month = cboMonth.SelectedIndex == 0 ? (int?)null : (int?)cboMonth.SelectedIndex;
            string type = cboTypeFilter.SelectedIndex == 0 ? null : cboTypeFilter.Text;

            var list = DatabaseHelper.GetAll(year, month, type);

            dgv.Columns.Clear();
            dgv.Rows.Clear();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",       HeaderText = "ID",     Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date",     HeaderText = "日期",    FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type",     HeaderText = "類型",    FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "分類",    FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount",   HeaderText = "金額 (NT$)", FillWeight = 18 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Note",     HeaderText = "備註",    FillWeight = 42 });

            foreach (var t in list)
            {
                int idx = dgv.Rows.Add(t.Id, t.Date.ToString("yyyy/MM/dd"), t.Type, t.Category,
                    t.Amount.ToString("N0"), t.Note);
                dgv.Rows[idx].DefaultCellStyle.ForeColor =
                    t.Type == "收入" ? Color.FromArgb(39, 174, 96) : Color.FromArgb(192, 57, 43);
            }

            // Summary
            int sy = year  ?? DateTime.Today.Year;
            int sm = month ?? DateTime.Today.Month;
            var (income, expense) = DatabaseHelper.GetMonthSummary(sy, sm);
            lblIncomeVal.Text  = $"${income:N0}";
            lblExpenseVal.Text = $"${expense:N0}";
            decimal balance = income - expense;
            lblBalanceVal.Text  = $"${balance:N0}";

            int y2 = month.HasValue ? sy : DateTime.Today.Year;
            int m2 = month ?? DateTime.Today.Month;
            lblMonthTitle.Text = $"{y2}年{m2:D2}月 摘要";
            lblStatus.Text = $"共 {list.Count} 筆記錄";
        }

        // ── Toolbar handlers ──────────────────────────────────────────────────────
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var dlg = new AddEditForm())
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    DatabaseHelper.Insert(dlg.Result);
                    LoadData();
                    SetStatus("新增成功 ✓");
                }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            int id = (int)dgv.CurrentRow.Cells["Id"].Value;
            var list = DatabaseHelper.GetAll();
            var t = list.Find(x => x.Id == id);
            if (t == null) return;

            using (var dlg = new AddEditForm(t))
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    DatabaseHelper.Update(dlg.Result);
                    LoadData();
                    SetStatus("修改成功 ✓");
                }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            if (MessageBox.Show("確定要刪除這筆記錄嗎？", "確認刪除",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            int id = (int)dgv.CurrentRow.Cells["Id"].Value;
            DatabaseHelper.Delete(id);
            LoadData();
            SetStatus("刪除成功 ✓");
        }

        private void SetStatus(string msg)
        {
            lblStatus.Text = msg;
            var t = new System.Windows.Forms.Timer { Interval = 2500 };
            t.Tick += (s, e) => { lblStatus.Text = $"共 {dgv.Rows.Count} 筆記錄"; t.Stop(); };
            t.Start();
        }
    }
}
