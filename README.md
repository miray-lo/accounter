# accounter
💰 個人記帳系統
一個使用 C# WinForms + SQLite 開發的個人財務記帳應用程式。
📸 功能截圖
> （請在完成後替換為實際截圖）
✨ 主要功能
功能	說明
新增記錄	輸入日期、分類、金額、備註
編輯記錄	修改已存在的收支記錄
刪除記錄	移除不需要的記錄
篩選查詢	依年份、月份、類型篩選
月份摘要	顯示當月收入、支出、結餘
圖表分析	圓餅圖顯示各分類支出/收入比例
資料持久化	所有資料存於本機 SQLite 資料庫
🛠️ 開發環境
語言：C# (.NET Framework 4.7.2)
框架：Windows Forms
資料庫：SQLite（System.Data.SQLite NuGet 套件）
IDE：Visual Studio 2022
🚀 執行方式
方法一：直接開啟專案
安裝 Visual Studio 2022（需含 .NET 桌面開發工作負載）
Clone 此專案：
```
   git clone https://github.com/你的帳號/AccountingSystem.git
   ```
開啟 `AccountingSystem.sln`
在「方案總管」右鍵點選專案 → 「管理 NuGet 套件」→ 安裝 `System.Data.SQLite`
按 F5 執行
方法二：還原 NuGet 套件後執行
```
nuget restore AccountingSystem.sln
```
接著在 Visual Studio 按 Ctrl+F5
📁 專案結構
```
AccountingSystem/
├── Data/
│   └── DatabaseHelper.cs    # SQLite CRUD 操作
├── Forms/
│   ├── MainForm.cs          # 主視窗
│   ├── AddEditForm.cs       # 新增/編輯對話框
│   └── ChartForm.cs         # 圖表分析視窗
├── Models/
│   └── Transaction.cs       # 資料模型
├── Properties/
│   └── AssemblyInfo.cs
├── Program.cs               # 程式進入點
├── .gitignore
└── README.md
```
📊 資料庫結構
Transactions 表
欄位	類型	說明
Id	INTEGER PK	自動編號
Date	TEXT	日期 (yyyy-MM-dd)
Type	TEXT	收入 / 支出
Category	TEXT	分類名稱
Amount	REAL	金額
Note	TEXT	備註
👤 作者
學號：1133512　姓名：羅健安
