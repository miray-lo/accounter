using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using AccountingSystem.Data;

namespace AccountingSystem.Forms
{
    public class ChartForm : Form
    {
        private Chart chart;
        private ComboBox cboYear, cboMonth, cboType;
        private Label lblNoData;

        public ChartForm()
        {
            InitializeComponent();
            LoadChart();
        }

        private void InitializeComponent()
        {
            Text = "圖表分析";
            Size = new Size(680, 560);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            Font = new Font("Microsoft JhengHei", 10f);

            // ── Filter bar ────────────────────────────────────────────────────────
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(245, 245, 245) };

            var lblYear = new Label { Text = "年份", Location = new Point(15, 15), AutoSize = true };
            cboYear = new ComboBox { Location = new Point(55, 12), Size = new Size(80, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            for (int y = DateTime.Today.Year; y >= DateTime.Today.Year - 5; y--)
                cboYear.Items.Add(y);
            cboYear.SelectedIndex = 0;

            var lblMonth = new Label { Text = "月份", Location = new Point(150, 15), AutoSize = true };
            cboMonth = new ComboBox { Location = new Point(190, 12), Size = new Size(70, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            for (int m = 1; m <= 12; m++) cboMonth.Items.Add(m + " 月");
            cboMonth.SelectedIndex = DateTime.Today.Month - 1;

            var lblType = new Label { Text = "類型", Location = new Point(275, 15), AutoSize = true };
            cboType = new ComboBox { Location = new Point(315, 12), Size = new Size(80, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cboType.Items.AddRange(new object[] { "支出", "收入" });
            cboType.SelectedIndex = 0;

            var btnRefresh = new Button
            {
                Text = "更新圖表", Location = new Point(415, 10), Size = new Size(90, 30),
                BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadChart();

            pnlTop.Controls.AddRange(new Control[] { lblYear, cboYear, lblMonth, cboMonth, lblType, cboType, btnRefresh });

            // ── Chart ─────────────────────────────────────────────────────────────
            chart = new Chart { Dock = DockStyle.Fill };
            var area = new ChartArea("main");
            chart.ChartAreas.Add(area);
            var legend = new Legend("leg") { Docking = Docking.Bottom };
            chart.Legends.Add(legend);

            lblNoData = new Label
            {
                Text = "本月無資料", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill, Font = new Font("Microsoft JhengHei", 14f),
                ForeColor = Color.Gray, Visible = false
            };

            Controls.Add(lblNoData);
            Controls.Add(chart);
            Controls.Add(pnlTop);
        }

        private void LoadChart()
        {
            int year  = (int)cboYear.SelectedItem;
            int month = cboMonth.SelectedIndex + 1;
            string type = cboType.Text;

            var data = DatabaseHelper.GetCategoryTotals(year, month, type);

            chart.Series.Clear();
            if (data.Count == 0)
            {
                chart.Visible = false;
                lblNoData.Visible = true;
                return;
            }
            chart.Visible = true;
            lblNoData.Visible = false;

            var series = new Series("amount") { ChartType = SeriesChartType.Pie };
            series["PieLabelStyle"] = "Outside";
            series["PieLineColor"]  = "Black";

            foreach (var kv in data)
            {
                int idx = series.Points.AddXY(kv.Key, kv.Value);
                series.Points[idx].Label = $"{kv.Key}\n{kv.Value:N0}";
            }
            chart.Series.Add(series);
            chart.Titles.Clear();
            chart.Titles.Add(new Title($"{year}/{month:D2} {type}分析",
                Docking.Top, new Font("Microsoft JhengHei", 13f, FontStyle.Bold), Color.FromArgb(44, 62, 80)));
        }
    }
}
