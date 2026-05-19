# PharmaDesk

Aplicatie desktop WPF .NET 8 pentru farmacie online, cu roluri `Admin`, `Pharmacist` si `User`.

## Conturi demo

- Admin: `admin` / `Admin123!`
- Farmacist: `farmacist` / `Farmacist123!`
- Client: `client` / `Client123!`

La prima rulare aplicatia creeaza baza de date prin EF Core `EnsureCreatedAsync` si adauga rolurile, utilizatorii demo, categoriile si produse initiale.

## Configurare MySQL

1. Instaleaza MySQL Server si creeaza un utilizator cu drepturi pe baza `PharmaDeskDB`.
2. Editeaza `appsettings.json`:

```json
"DefaultConnection": "Server=localhost;Port=3306;Database=PharmaDeskDB;User=root;Password=parola_ta;TreatTinyAsBoolean=true;"
```

3. Ruleaza proiectul din Visual Studio 2022 sau cu:

```powershell
dotnet run
```

Scriptul SQL complet este in `Database/PharmaDeskDB.sql`.

## Functionalitati

- Client: inregistrare, login, catalog cu cautare si categorii, carduri cu rating/badge-uri, cos persistent in baza de date, checkout cu plata simulata, incarcare prescriptie PDF pentru produse RX, istoric comenzi si facturi PDF.
- Farmacist: dashboard, gestionare medicamente, preturi, stoc, prescriptii, categorii si comenzi primite.
- Admin: tot ce poate farmacistul, plus utilizatori, rapoarte PDF/Excel si audit log.

## Tehnologii

- .NET 8, WPF, MVVM cu CommunityToolkit.Mvvm
- Dependency Injection cu Microsoft.Extensions.Hosting
- Entity Framework Core + Pomelo.EntityFrameworkCore.MySql
- BCrypt.Net-Next pentru parole
- iText7 pentru facturi PDF
- EPPlus pentru export Excel
- MaterialDesignThemes pentru baza vizuala

## Migrare EF Core

Proiectul include `PharmaDeskDbContextFactory`, astfel incat poti genera migrari din Visual Studio Package Manager Console sau terminal:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Pentru rulare simpla, aplicatia creeaza automat schema daca baza este goala.

Daca vezi eroarea `Access denied for user 'root'@'localhost' (using password: NO)`, inseamna ca `Password=` este gol in `appsettings.json`, dar MySQL cere parola pentru `root`. Completeaza parola reala sau creeaza un utilizator dedicat, de exemplu:

```sql
CREATE USER 'pharmadesk'@'localhost' IDENTIFIED BY 'PharmaDesk123!';
GRANT ALL PRIVILEGES ON PharmaDeskDB.* TO 'pharmadesk'@'localhost';
FLUSH PRIVILEGES;
```

Apoi foloseste:

```json
"DefaultConnection": "Server=localhost;Port=3306;Database=PharmaDeskDB;User=pharmadesk;Password=PharmaDesk123!;TreatTinyAsBoolean=true;"
```

## Output local

- Facturi: `Invoices/`
- Prescriptii incarcate: `Prescriptions/`
- Rapoarte: `Reports/`
- Email dummy: `Logs/dummy-email.log`
