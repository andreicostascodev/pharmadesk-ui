using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PharmaDesk.Models;

namespace PharmaDesk.Data;

public class DatabaseInitializer(PharmaDeskDbContext db, ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync()
    {
        try
        {
            await db.Database.EnsureCreatedAsync();
            await SeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed");
            throw;
        }
    }

    private async Task SeedAsync()
    {
        await EnsureRoleAsync("Admin");
        await EnsureRoleAsync("Pharmacist");
        await EnsureRoleAsync("User");

        var adminRoleId = await db.Roles.Where(x => x.Name == "Admin").Select(x => x.Id).FirstAsync();
        var pharmacistRoleId = await db.Roles.Where(x => x.Name == "Pharmacist").Select(x => x.Id).FirstAsync();
        var userRoleId = await db.Roles.Where(x => x.Name == "User").Select(x => x.Id).FirstAsync();

        await EnsureUserAsync("admin", "Admin123!", "admin@pharmadesk.local", "Administrator PharmaDesk", adminRoleId);
        await EnsureUserAsync("farmacist", "Farmacist123!", "farmacist@pharmadesk.local", "Farmacist Demo", pharmacistRoleId);
        await EnsureUserAsync("client", "Client123!", "client@pharmadesk.local", "Client Demo", userRoleId);

        await SeedCatalogAsync();

        if (!await db.Settings.AnyAsync())
        {
            db.Settings.AddRange(
                new Setting { SettingKey = "CompanyName", SettingValue = "PharmaDesk" },
                new Setting { SettingKey = "TaxRate", SettingValue = "0.09" },
                new Setting { SettingKey = "Currency", SettingValue = "RON" });
        }

        await db.SaveChangesAsync();
    }

    private async Task EnsureRoleAsync(string name)
    {
        if (!await db.Roles.AnyAsync(x => x.Name == name))
        {
            db.Roles.Add(new Role { Name = name });
            await db.SaveChangesAsync();
        }
    }

    private async Task EnsureUserAsync(string username, string password, string email, string fullName, int roleId)
    {
        if (await db.Users.AnyAsync(x => x.Username == username)) return;
        db.Users.Add(new User
        {
            Username = username,
            Email = email,
            FullName = fullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RoleId = roleId,
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedCatalogAsync()
    {
        var categoryNames = new[]
        {
            "Durere si febra", "Raceala si gripa", "Vitamine si imunitate", "Digestie", "Alergii",
            "Dermatocosmetice", "Mama si copilul", "Cardiovascular", "Diabet", "Ochi si urechi",
            "Ingrijire orala", "Prim ajutor", "Sport si nutritie", "Naturiste"
        };

        foreach (var name in categoryNames)
        {
            await EnsureCategoryAsync(name, $"Selectie premium pentru {name.ToLower()}.");
        }

        var cats = await db.Categories.ToDictionaryAsync(x => x.Name, x => x.Id);
        var seeds = new[]
        {
            Product("Paracetamol Forte 500mg", "Paracetamol", "594100000001", cats["Durere si febra"], "Comprimate", "500mg", 14.90m, 120, false, true, false, 0, "Analgezic si antipiretic pentru dureri usoare si febra.", "paracetamol-forte.png", 4.7m),
            Product("Ibuprofen Rapid 400mg", "Ibuprofen", "594100000002", cats["Durere si febra"], "Capsule moi", "400mg", 22.50m, 85, false, false, true, 20, "Anti-inflamator cu actiune rapida.", "ibuprofen-rapid.png", 4.8m),
            Product("Coldrex Plus", "Paracetamol + vitamina C", "594100000009", cats["Raceala si gripa"], "Plicuri", "10 plicuri", 34.90m, 76, false, true, true, 12, "Pentru simptome de raceala si gripa.", "coldrex-plus.png", 4.6m),
            Product("Nurofen Sinus", "Ibuprofen + pseudoefedrina", "594100000010", cats["Raceala si gripa"], "Comprimate", "200mg", 28.50m, 52, false, false, true, 10, "Ajutor pentru durere si congestie nazala.", "nurofen-sinus.png", 4.5m),
            Product("Vitamina C + Zinc", "Acid ascorbic", "594100000003", cats["Vitamine si imunitate"], "Comprimate efervescente", "1000mg", 31.99m, 160, false, true, true, 15, "Suport zilnic pentru imunitate.", "vitamina-c-zinc.png", 4.9m),
            Product("Magneziu B6 Premium", "Magneziu", "594100000004", cats["Vitamine si imunitate"], "Tablete", "375mg", 39.90m, 60, false, false, false, 0, "Pentru oboseala si functionarea normala a sistemului nervos.", "magneziu-b6.png", 4.5m),
            Product("Calciu D3 Forte", "Calciu + colecalciferol", "594100000014", cats["Vitamine si imunitate"], "Comprimate", "600mg", 42.40m, 48, false, false, false, 0, "Suport pentru oase si dinti.", "calciu-d3.png", 4.4m),
            Product("Ferro Plus", "Fier", "594100000036", cats["Vitamine si imunitate"], "Capsule", "30 capsule", 33.50m, 39, false, false, false, 0, "Suport nutritional in perioade de oboseala.", "ferro-plus.png", 4.3m),
            Product("Probiotic 10 Flora", "Lactobacillus", "594100000011", cats["Digestie"], "Capsule", "10 tulpini", 46.00m, 62, false, true, false, 0, "Echilibru pentru flora intestinala.", "probiotic-10.png", 4.8m),
            Product("Omeprazol Control", "Omeprazol", "594100000012", cats["Digestie"], "Capsule gastrorezistente", "20mg", 26.80m, 44, false, false, true, 8, "Control pentru arsuri gastrice frecvente.", "omeprazol-control.png", 4.4m),
            Product("Cetirizina Alergii", "Cetirizina", "594100000013", cats["Alergii"], "Comprimate", "10mg", 18.90m, 72, false, true, false, 0, "Amelioreaza stranutul si mancarimile alergice.", "cetirizina-alergii.png", 4.5m),
            Product("DermaCare Repair 50ml", "Panthenol", "594100000005", cats["Dermatocosmetice"], "Crema", "50ml", 48.00m, 45, false, true, false, 0, "Crema calmanta pentru piele sensibila.", "dermacare-crema.png", 4.7m),
            Product("Hyaluron Ser 30ml", "Acid hialuronic", "594100000008", cats["Dermatocosmetice"], "Ser", "30ml", 72.00m, 40, false, true, true, 25, "Hidratare intensa si textura usoara.", "hyaluron-ser.png", 4.9m),
            Product("Colagen Beauty", "Colagen hidrolizat", "594100000021", cats["Dermatocosmetice"], "Pulbere", "300g", 89.00m, 35, false, false, true, 18, "Formula pentru piele, par si unghii.", "colagen-beauty.png", 4.6m),
            Product("SunCare SPF 50", "Filtre UVA/UVB", "594100000022", cats["Dermatocosmetice"], "Crema", "75ml", 58.00m, 55, false, true, true, 15, "Protectie solara avansata.", "spf-50.png", 4.7m),
            Product("Sirop Tuse Junior", "Extract plante", "594100000006", cats["Mama si copilul"], "Sirop", "150ml", 27.50m, 33, false, false, true, 10, "Formula blanda pentru copii.", "sirop-tuse-junior.png", 4.4m),
            Product("Baby Drops D3", "Vitamina D3", "594100000023", cats["Mama si copilul"], "Picaturi", "10ml", 29.90m, 50, false, true, false, 0, "Vitamina D3 pentru copii.", "baby-drops.png", 4.7m),
            Product("Prenatal Plus", "Complex prenatal", "594100000024", cats["Mama si copilul"], "Comprimate", "30 comprimate", 64.90m, 36, false, false, true, 12, "Suport nutritional in sarcina.", "prenatal-plus.png", 4.6m),
            Product("Omega 3 Ultra", "Ulei de peste", "594100000015", cats["Cardiovascular"], "Capsule moi", "1000mg", 54.90m, 80, false, false, true, 10, "Suport pentru inima si circulatie.", "omega-3.png", 4.8m),
            Product("Tensio Plus", "Extract paducel", "594100000016", cats["Cardiovascular"], "Tablete", "60 tablete", 49.90m, 28, false, false, false, 0, "Suport pentru tensiune normala.", "tensioplus.png", 4.3m),
            Product("Gluco Balance", "Crom + plante", "594100000017", cats["Diabet"], "Capsule", "30 capsule", 44.00m, 31, false, true, false, 0, "Suport pentru metabolismul glucozei.", "gluco-balance.png", 4.4m),
            Product("Insulin Care RX", "Insulina", "594100000018", cats["Diabet"], "Pen injectabil", "100UI/ml", 118.00m, 16, true, false, false, 0, "Produs cu eliberare pe baza de prescriptie.", "insulin-care.png", 4.7m),
            Product("Lacrimi Artificiale", "Hipromeloza", "594100000028", cats["Ochi si urechi"], "Picaturi", "10ml", 24.90m, 58, false, false, false, 0, "Confort pentru ochi uscati.", "lacrimi-artificiale.png", 4.5m),
            Product("OtoCalm Picaturi", "Glicerol", "594100000029", cats["Ochi si urechi"], "Picaturi auriculare", "10ml", 22.30m, 38, false, false, true, 7, "Ingrijire pentru disconfort auricular.", "otocalm.png", 4.2m),
            Product("Denta Sensitive", "Fluor", "594100000030", cats["Ingrijire orala"], "Pasta de dinti", "75ml", 19.50m, 95, false, true, false, 0, "Protectie pentru dinti sensibili.", "pasta-sensitive.png", 4.5m),
            Product("Mouth Fresh", "Clorhexidina", "594100000031", cats["Ingrijire orala"], "Apa de gura", "250ml", 23.90m, 67, false, false, true, 9, "Respiratie proaspata si ingrijire gingivala.", "apa-de-gura.png", 4.4m),
            Product("Termometru Digital", "Dispozitiv medical", "594100000025", cats["Prim ajutor"], "Dispozitiv", "1 buc", 35.00m, 42, false, true, false, 0, "Masurare rapida si precisa.", "thermometer-digital.png", 4.6m),
            Product("Bandaje Sterile", "Set pansamente", "594100000026", cats["Prim ajutor"], "Set", "20 buc", 16.90m, 120, false, false, false, 0, "Set steril pentru mici accidente.", "bandaje-sterile.png", 4.3m),
            Product("Dezinfectant Spray", "Alcool 70%", "594100000027", cats["Prim ajutor"], "Spray", "100ml", 18.00m, 88, false, false, true, 10, "Igiena rapida pentru piele.", "dezinfectant.png", 4.5m),
            Product("Whey Protein", "Proteina zer", "594100000032", cats["Sport si nutritie"], "Pulbere", "900g", 119.00m, 25, false, true, true, 20, "Suport pentru masa musculara.", "proteina-whey.png", 4.7m),
            Product("Creatina Pure", "Creatina monohidrata", "594100000033", cats["Sport si nutritie"], "Pulbere", "300g", 69.00m, 34, false, false, false, 0, "Performanta si recuperare.", "creatina.png", 4.8m),
            Product("Electro Lytes", "Electroliti", "594100000019", cats["Sport si nutritie"], "Plicuri", "12 plicuri", 37.90m, 46, false, true, false, 0, "Hidratare dupa efort.", "electroliti.png", 4.4m),
            Product("Echinacea Natural", "Echinacea", "594100000034", cats["Naturiste"], "Capsule", "60 capsule", 36.00m, 49, false, false, true, 12, "Suport natural pentru sezonul rece.", "echinacea.png", 4.4m),
            Product("Valeriana Calm", "Extract valeriana", "594100000035", cats["Naturiste"], "Comprimate", "30 comprimate", 27.90m, 57, false, false, false, 0, "Relaxare si somn linistit.", "valeriana.png", 4.3m),
            Product("Melatonina Somn", "Melatonina", "594100000020", cats["Naturiste"], "Comprimate", "1mg", 29.00m, 60, false, true, true, 10, "Ajuta la reducerea timpului de adormire.", "melatonina.png", 4.6m),
            Product("Antibiotic RX 875mg", "Amoxicilina", "594100000007", cats["Durere si febra"], "Comprimate filmate", "875mg", 56.00m, 18, true, false, false, 0, "Produs eliberat numai pe baza de prescriptie.", "antibiotic-rx.png", 4.6m)
        };

        foreach (var product in seeds)
        {
            var existing = await db.Medicines.FirstOrDefaultAsync(x => x.Barcode == product.Barcode);
            if (existing is null)
            {
                db.Medicines.Add(product);
            }
            else
            {
                existing.CategoryId = product.CategoryId;
                existing.ImageUrl = product.ImageUrl;
                existing.Description = product.Description;
                existing.Rating = product.Rating;
                existing.IsNew = product.IsNew;
                existing.IsPromotion = product.IsPromotion;
                existing.DiscountPercent = product.DiscountPercent;
                existing.StockQuantity = Math.Max(existing.StockQuantity, product.StockQuantity);
                existing.ReorderLevel = product.ReorderLevel;
            }
        }
    }

    private async Task EnsureCategoryAsync(string name, string description)
    {
        if (!await db.Categories.AnyAsync(x => x.Name == name))
        {
            db.Categories.Add(new Category { Name = name, Description = description });
            await db.SaveChangesAsync();
        }
    }

    private static Medicine Product(string name, string generic, string barcode, int categoryId, string form, string strength, decimal price, int stock, bool rx, bool isNew, bool promo, int discount, string description, string imageFile, decimal rating) => new()
    {
        Name = name,
        GenericName = generic,
        Barcode = barcode,
        CategoryId = categoryId,
        DosageForm = form,
        Strength = strength,
        UnitPrice = price,
        StockQuantity = stock,
        ReorderLevel = 20,
        IsPrescriptionRequired = rx,
        IsActive = true,
        ImageUrl = $"pack://application:,,,/Assets/Products/{imageFile}",
        Description = description,
        IsNew = isNew,
        IsPromotion = promo,
        DiscountPercent = discount,
        Rating = rating,
        CreatedAt = DateTime.UtcNow
    };
}
