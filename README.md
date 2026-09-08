# Component Inventory Manager

A small full-stack project for tracking electronics components and parts
across personal projects (Arduino, ESP32, PLC labs, robotics builds, etc.) —
built to solve a real problem: knowing what parts you actually have before
starting a new build.

It's split into two front ends sharing one MySQL database:

- **Desktop app** (C# / Windows Forms) — the main tool: add, edit, delete,
  search, and filter components, with low-stock rows highlighted automatically.
- **Web dashboard** (PHP) — a lightweight read-only view of the same data,
  reachable from a browser without opening the desktop app.

## Setup

### 1. Database
Run `database/schema.sql` in MySQL to create the database, tables, and sample data.

### 2. Desktop app (C#)
1. Open `desktop-app/ComponentInventoryManager.sln` in Visual Studio.
2. Install the **MySql.Data** NuGet package (Manage NuGet Packages → search "MySql.Data" → Install).
3. Update the `Server`, `User`, and `Password` values in `Database.cs` to match your MySQL setup.
4. Run the project (F5).

### 3. Web dashboard (PHP)
1. Update the connection details in `web-dashboard/db.php`.
2. From the `web-dashboard` folder, run `php -S localhost:8000`.
3. Open `http://localhost:8000` in a browser.

## What each part demonstrates

- **SQL**: table design with foreign keys and a many-to-many junction table (`project_components`).
- **C#**: a full Windows Forms CRUD app with `DataGridView` binding, parameterized queries, and a reusable Add/Edit dialog.
- **PHP**: server-side rendering with prepared statements, reading the same data the desktop app writes.

## Notes

Both the C# and PHP sides use parameterized queries instead of string-concatenated SQL — this prevents SQL injection.
