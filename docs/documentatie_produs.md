# 3.2 Testarea sistemului

Testarea aplicației PharmaDesk a fost realizată prin teste manuale funcționale, acoperind principalele fluxuri de utilizare și cazurile limită identificate în etapa de proiectare.

**Testarea modulului de autentificare** a verificat comportamentul aplicației pentru mai multe scenarii: înregistrarea cu succes a unui cont nou cu date valide, tentativa de înregistrare cu un username deja existent în sistem, autentificarea cu credențiale corecte, autentificarea cu parolă incorectă și autentificarea cu un username inexistent. În toate cazurile de eroare, aplicația a afișat mesajul corespunzător fără a produce excepții negestionate.

**Testarea modulului de catalog și coș** a acoperit căutarea produselor după denumire, filtrarea pe categorii, adăugarea și eliminarea produselor din coș, modificarea cantității cu actualizare automată a totalului, și finalizarea comenzii cu generarea facturii PDF. S-a verificat că produsele cu stoc zero nu pot fi adăugate în coș și că prețurile afișate corespund celor din baza de date.

**Testarea navigării** a confirmat că toate tranzițiile între ecrane se realizează corect prin sistemul ContentControl + DataTemplate, că butonul de deconectare șterge sesiunea curentă și revine la ecranul de autentificare, și că utilizatorii fără rol de Admin nu pot accesa panourile de administrare.

**Testarea dashboard-ului Admin** a urmărit afișarea corectă a KPI-urilor (total produse, comenzi azi, stoc scăzut, venituri azi), a graficului de venituri zilnice pe 7/30/90 de zile, a donut-ului de status comenzi și a tabelului cu comenzi recente. S-a verificat că datele reflectă starea reală din baza de date MySQL la fiecare încărcare.

**Testarea modulului de gestionare produse** a confirmat că adăugarea, editarea și ștergerea unui produs se reflectă imediat în catalog, că validarea câmpurilor respinge prețuri negative sau stoc negativ, și că zona de upload imagine respinge fișiere cu extensie invalidă.

**Testarea temei vizuale** a confirmat că schimbarea temei light/dark prin butonul din bara laterală este aplicată imediat la nivel de aplicație, că preferința este persistată în `appsettings.json` și că la o nouă pornire a aplicației tema salvată este restaurată automat.

**Testarea rapoartelor și exporturilor** a verificat generarea corectă a fișierelor Excel (EPPlus) și PDF (iText7), că acestea conțin datele corespunzătoare perioadei selectate și că sunt salvate în folderul `Reports/` fără erori.

În urma testelor efectuate nu au fost identificate defecte critice. Comportamentul aplicației a corespuns cerințelor funcționale și nefuncționale definite, confirmând corectitudinea implementării.

---

# 4. Documentarea produsului realizat

## 4.1 Denumirea aplicației

**PharmaDesk** — Aplicație Desktop pentru Gestionarea unei Farmacii Online

## 4.2 Descrierea generală

PharmaDesk este o aplicație desktop dezvoltată pentru sistemul de operare Windows, destinată administrării complete a unei farmacii online. Aplicația oferă trei roluri distincte de utilizator — Client, Farmacist și Administrator — fiecare cu un set specific de funcționalități. Clienții pot naviga în catalog, adăuga produse în coș, plasa comenzi și încărca prescripții medicale. Farmaciștii pot gestiona stocul și comenzile primite, iar administratorii au acces complet la rapoarte, utilizatori și jurnalul de audit. Interfața adoptă un design modern tip bento-grid cu paleta navy (#14213D) și amber (#FCA311), cu suport pentru teme light și dark.

## 4.3 Scopul aplicației

Scopul principal al aplicației PharmaDesk este de a oferi un instrument complet pentru:

- Gestionarea catalogului de medicamente cu categorii, prețuri și stoc;
- Procesarea comenzilor de la plasare până la livrare;
- Monitorizarea stocului scăzut și generarea de alerte automate;
- Vizualizarea grafică a veniturilor și statisticilor de vânzări prin LiveCharts2;
- Administrarea utilizatorilor și a rolurilor din sistem;
- Exportul rapoartelor în format Excel și PDF;
- Asigurarea securității prin criptarea parolelor cu BCrypt.

## 4.4 Tehnologii utilizate

| Categorie | Tehnologie |
|-----------|-----------|
| Limbaj de programare | C# 12 |
| Platformă | .NET 8 (net8.0-windows) |
| Framework UI | WPF (Windows Presentation Foundation) |
| Limbaj de marcare | XAML |
| Pattern arhitectural | MVVM cu CommunityToolkit.Mvvm |
| Injecție dependențe | Microsoft.Extensions.Hosting |
| Bază de date | MySQL 8.x |
| ORM | Entity Framework Core + Pomelo.EntityFrameworkCore.MySql |
| Criptare parole | BCrypt.Net-Next |
| Export PDF | iText7 |
| Export Excel | EPPlus |
| Grafice | LiveChartsCore.SkiaSharpView.WPF |
| Mediu de dezvoltare | Visual Studio 2022 / dotnet CLI |
| Control versiune | Git + GitHub |

## 4.5 Arhitectura aplicației

Aplicația este organizată pe o structură modulară, separând clar responsabilitățile:

```
PharmaDesk/
├── App.xaml / App.xaml.cs              → Punct de intrare, container DI, înregistrare servicii
├── MainWindow.xaml / .cs               → Fereastra principală cu sidebar navy și navigare MVVM
├── Models/
│   ├── DomainModels.cs                 → Entități EF Core (User, Medicine, Order, Category, AuditLog)
│   ├── TopProductItem.cs               → Model pentru top produse vândute
│   └── RecentOrderItem.cs              → Model pentru comenzi recente în dashboard
├── ViewModels/
│   ├── MainViewModel.cs                → Navigare, sesiune, toggle temă
│   ├── AdminDashboardViewModel.cs      → KPI-uri, grafice, date live din MySQL
│   └── ViewModels.cs                   → Toate celelalte ViewModel-uri (13 ecrane)
├── Views/
│   ├── LoginView / RegisterView        → Autentificare și înregistrare
│   ├── AdminDashboardView              → Tabloul de bord cu bento-grid
│   ├── AdminOrdersView                 → Gestiunea comenzilor cu filtre
│   ├── ProductManagementView           → CRUD medicamente + upload imagine
│   ├── CategoryManagementView          → Gestiunea categoriilor
│   ├── UserManagementView              → Gestiunea utilizatorilor și rolurilor
│   ├── AuditLogView                    → Jurnal de activitate cu filtre
│   ├── ReportsView                     → Grafice și export rapoarte
│   ├── UserHomeView                    → Catalog produse cu căutare și filtre
│   ├── CartView                        → Coș de cumpărături
│   ├── OrderHistoryView                → Istoricul comenzilor clientului
│   └── ProfileView                     → Profilul utilizatorului
├── Services/
│   ├── AuthService.cs                  → Autentificare și înregistrare
│   ├── CatalogService.cs               → Catalog produse și categorii
│   ├── CartService.cs                  → Coș de cumpărături persistent
│   ├── OrderService.cs                 → Procesarea comenzilor
│   ├── ReportService.cs                → Generare rapoarte Excel/PDF
│   ├── AuditService.cs                 → Înregistrare jurnal de activitate
│   ├── ThemeService.cs                 → Comutare temă light/dark cu persistare
│   └── ToastService.cs                 → Notificări in-app
├── Styles/
│   ├── Shared.xaml                     → Tokens, BentoCard, butoane, DataGrid
│   ├── LightTheme.xaml                 → Culori temă luminoasă
│   ├── DarkTheme.xaml                  → Culori temă întunecată
│   └── NavigationSidebar.xaml          → Stiluri sidebar și header
├── Data/
│   └── PharmaDeskDbContext.cs          → Context EF Core
├── Helpers/
│   └── ValueConverters.cs              → Convertoare XAML (status, brush, inițiale)
└── appsettings.json                    → Configurare conexiune MySQL și temă salvată
```

## 4.6 Funcționalitățile principale

### 4.6.1 Autentificare și înregistrare

- Ecran de login cu username și parolă;
- Înregistrare cont nou cu validare date (username unic, parolă minimă);
- Parole stocate criptat cu BCrypt (cost factor 12);
- Redirecționare automată pe dashboard-ul corespunzător rolului după login;
- Buton de logout cu ștergerea sesiunii curente.

### 4.6.2 Dashboard Admin (bento-grid)

- **KPI-uri în timp real:** total produse active, comenzi azi, produse cu stoc scăzut, venituri azi;
- **Grafic venituri zilnice** (LiveCharts2 ColumnSeries) cu toggle 7d / 30d / 90d;
- **Donut comenzi** (LiveCharts2 PieSeries) segmentat pe Pending / Completed / Cancelled;
- **Top 5 produse vândute** cu bare de progres proporționale;
- **Tabel comenzi recente** cu avatar inițiale client, status badge colorat și total.

### 4.6.3 Catalog și coș (rol Client)

- Grilă de produse cu imagine, denumire, categorie, preț și buton „Adaugă în coș";
- Căutare live după denumire și filtrare pe categorii (pills toggle);
- Coș persistent în baza de date cu stepper cantitate (−/+);
- Sumar comandă: subtotal, taxă livrare, total;
- Checkout cu generare automată număr comandă și factură PDF.

### 4.6.4 Gestionare produse (rol Admin/Farmacist)

- Tabel cu toate medicamentele: denumire, categorie, preț, stoc;
- Formular inline pentru adăugare și editare produs;
- Zonă upload imagine cu previzualizare;
- Ștergere cu confirmare;
- Alertă vizuală pentru produse cu stoc sub nivelul minim.

### 4.6.5 Gestionare comenzi

- Filtre rapide: Toate / Pending / Completed / Cancelled cu badge-uri numerice;
- Actualizare status comandă direct din tabel;
- Vizualizare detalii comandă (produse, cantități, total);
- Export comenzi în Excel.

### 4.6.6 Rapoarte

- Grafice venituri lunare și distribuție comenzi (LiveCharts2);
- KPI-uri sumare: venituri totale, total comenzi, produse, utilizatori;
- Export raport complet în **Excel** (EPPlus) și **PDF** (iText7).

### 4.6.7 Jurnal de audit

- Înregistrare automată a tuturor acțiunilor administrative (creare, modificare, ștergere);
- Filtrare după interval de date și tip acțiune;
- Afișare timestamp, utilizator responsabil, tabel afectat și ID înregistrare.

### 4.6.8 Temă light / dark

- Toggle din bara laterală cu aplicare imediată fără restart;
- Preferința este persistată în `appsettings.json`;
- La pornirea aplicației tema salvată este restaurată automat.

## 4.7 Securitate

- Parolele nu sunt stocate niciodată în clar — se folosește **BCrypt** cu salt aleator și cost factor 12;
- Accesul la funcționalitățile administrative este condiționat de rolul utilizatorului, verificat la fiecare navigare;
- Jurnalul de audit înregistrează toate modificările sensibile cu utilizatorul și timestamp-ul aferent;
- Conexiunea la baza de date folosește credențiale stocate local în `appsettings.json`, care nu este inclus în repository.

## 4.8 Fluxul utilizatorului

**Client:**
1. Pornire aplicație → ecran Login;
2. Autentificare sau înregistrare cont nou;
3. Navigare în catalog → căutare și filtrare produse;
4. Adăugare produse în coș → ajustare cantități;
5. Finalizare comandă → confirmare și factură PDF;
6. Vizualizare istoric comenzi și profil personal.

**Administrator:**
1. Autentificare cu cont Admin;
2. Dashboard cu KPI-uri și grafice live;
3. Gestionare produse, categorii, utilizatori și comenzi;
4. Generare și export rapoarte;
5. Consultare jurnal de audit.

## 4.9 Instalare și rulare (pentru dezvoltatori)

### Cerințe de sistem

- Windows 10 sau 11 (64-bit);
- .NET 8 SDK instalat — [descărcare](https://dotnet.microsoft.com/download/dotnet/8.0);
- MySQL Server 8.x sau XAMPP cu MySQL activ;
- Minimum 200 MB spațiu pe disc.

### Pași de rulare

```bash
# 1. Clonează repository-ul
git clone https://github.com/andreicostascodev/pharmadesk-ui.git

# 2. Intră în folder
cd pharmadesk-ui

# 3. Configurează conexiunea MySQL în appsettings.json
# Editează câmpul DefaultConnection cu datele tale

# 4. Rulează aplicația
dotnet run
```

La prima rulare, aplicația creează automat schema bazei de date prin EF Core `EnsureCreatedAsync` și populează datele inițiale (roluri, utilizatori demo, categorii, produse).

### Conturi demo (create automat)

| Rol | Username | Parolă |
|-----|----------|--------|
| Admin | `admin` | `Admin123!` |
| Farmacist | `farmacist` | `Farmacist123!` |
| Client | `client` | `Client123!` |

## 4.10 Instrucțiuni de rulare pentru utilizatori (fără cunoștințe de programare)

### Pasul 1 — Instalează XAMPP (baza de date)

1. Accesează [https://www.apachefriends.org](https://www.apachefriends.org) → **Download** → alege versiunea pentru **Windows**;
2. Rulează fișierul descărcat și apasă **Next** la tot. Asigură-te că **MySQL** este bifat în lista de componente;
3. După instalare, deschide **XAMPP Control Panel** (din Start Menu);
4. Pe rândul **MySQL** apasă butonul **Start** — devine verde când MySQL rulează.

### Pasul 2 — Instalează .NET 8 Desktop Runtime

1. Accesează [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0);
2. Descarcă **".NET Desktop Runtime 8.0"** pentru Windows (x64);
3. Rulează fișierul descărcat și apasă **Install**. Așteaptă să se termine.

### Pasul 3 — Descarcă aplicația

1. Deschide [https://github.com/andreicostascodev/pharmadesk-ui](https://github.com/andreicostascodev/pharmadesk-ui) într-un browser;
2. Apasă butonul verde **Code** → **Download ZIP**;
3. Găsește fișierul `pharmadesk-ui-main.zip` în folderul **Downloads**;
4. Click dreapta pe el → **Extract All** → alege `C:\PharmaDesk` → apasă **Extract**.

### Pasul 4 — Configurează parola MySQL

Dacă XAMPP folosește parola implicită (goală), trebuie să o setezi la `1414`:

1. În XAMPP Control Panel, lângă MySQL apasă **Shell**;
2. În fereastra neagră care se deschide, scrie și apasă Enter:
   ```
   mysqladmin -u root password 1414
   ```
3. Închide fereastra Shell.

> **Alternativ:** dacă preferi să nu setezi o parolă, deschide fișierul `C:\PharmaDesk\appsettings.json` cu Notepad și schimbă `Password=1414` în `Password=`.

### Pasul 5 — Pornește aplicația

1. Apasă **Win + R** → scrie `cmd` → apasă **Enter**;
2. În fereastra neagră, scrie comenzile de mai jos (una câte una, apăsând Enter după fiecare):
   ```
   cd C:\PharmaDesk
   dotnet run
   ```
3. La prima rulare durează **30–60 de secunde** (se creează baza de date automat). Aplicația se va deschide într-o fereastră nouă.

### Pasul 6 — Folosește aplicația

- Autentifică-te cu unul din conturile demo de mai sus;
- **Admin** are acces complet: dashboard, produse, comenzi, utilizatori, rapoarte;
- **Client** poate naviga în catalog, adăuga în coș și plasa comenzi.

### Probleme frecvente

| Problemă | Soluție |
|----------|---------|
| Mesaj „dotnet is not recognized" | .NET nu este instalat — repetă Pasul 2 |
| Eroare de conectare la baza de date | Verifică că MySQL este pornit (verde în XAMPP) |
| Eroare „Access denied" la MySQL | Verifică că parola din `appsettings.json` este corectă |
| Aplicația nu se deschide | Asigură-te că ai instalat **Desktop Runtime**, nu doar Runtime simplu |
| Eroare „file not found" la `cd` | Verifică calea unde ai dezarhivat — adaptează comanda `cd` |

## 4.11 Repository GitHub

Codul sursă complet este disponibil public la:

🔗 [https://github.com/andreicostascodev/pharmadesk-ui](https://github.com/andreicostascodev/pharmadesk-ui)

## 4.12 Direcții viitoare de dezvoltare

- **Notificări automate** pentru stoc scăzut prin email sau push notification;
- **Modul de prescripții digitale** cu validare și aprobare de către farmacist;
- **Integrare plăți reale** prin Stripe sau PayU;
- **Versiune web** prin migrare la .NET MAUI Blazor sau ASP.NET Core;
- **Rapoarte avansate** cu comparații lunare și prognoze de stoc;
- **Sistem de fidelizare** pentru clienți recurenți cu puncte și reduceri.

## 4.13 Concluzie

PharmaDesk demonstrează aplicarea practică a conceptelor moderne de dezvoltare desktop în C#/WPF: arhitectură MVVM cu injecție de dependențe, teme vizuale dinamice, grafice interactive, gestionare securizată a datelor și o experiență de utilizare premium cu design bento-grid navy-amber. Aplicația acoperă integral fluxul unei farmacii online, de la autentificarea clientului până la rapoartele administrative și exportul de documente, toate conectate la o bază de date MySQL reală prin Entity Framework Core.

---

*© 2026 Andrei Costașco*
