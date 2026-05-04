# 💰 Cost Check - Budget & Finance Assistant PRO

Cost Check is a modern, responsive, and standalone desktop application for tracking personal finances, built with C# and .NET Windows Forms.

## ✨ Features
- **Dashboard & Pie Chart:** Visually analyze your expenses categorized by beautiful, interactive pie charts.
- **Advanced Filtering:** Filter transactions based on date ranges (Start & End dates) to view weekly or monthly summaries.
- **Categorization:** Pre-defined smart categories (Groceries, Rent, Entertainment, Pets, etc.) to understand your spending habits.
- **Multi-Language & Currency:** Fully localized in English and Turkish. Supports TL (₺), USD ($), and EUR (€).
- **Edit & Delete:** Safely edit past entries or delete them via context menu, double-click, or hotkeys.
- **CSV Export:** Export your filtered or entire financial history directly to Microsoft Excel / CSV.
- **Portable JSON Storage:** Your data is kept locally and safely in a clean JSON format.

## 🚀 How to Run
Go to the **Releases** tab to download the standalone `.exe` file. No installation is required—just double click and start managing your budget!

## 🛠️ Build from Source
If you wish to compile the project yourself:
1. Ensure you have the .NET SDK installed.
2. Run `dotnet run` to launch the application in development mode.
3. Run `dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true` to build a single standalone executable.
