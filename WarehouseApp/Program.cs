using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using WarehouseApp.Data;
using WarehouseApp.Models;
using WarehouseApp.Services;


var builder = WebApplication.CreateBuilder(args);

// ===== QUESTPDF – LICENCJA COMMUNITY =====
QuestPDF.Settings.License = LicenseType.Community;

// ===== KONFIGURACJA BAZY DANYCH =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ===== IDENTITY + ROLE =====
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<ILocationAllocator, SimpleLocationAllocator>();


var app = builder.Build();

// ===== SEED RÓL, ADMINA I DANYCH MAGAZYNOWYCH =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    // --- ROLE ---
    string[] roleNames = { "Administrator", "Pracownik magazynu", "Kierownik magazynu", "Audytor" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // --- ADMIN ---
    string adminEmail = "admin@warehouse.local";
    string adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "Administrator systemu"
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }
    }

    // --- LOKALIZACJE ---
    var seedLocations = new[]
    {
        new { Code = "A-01-01", Description = "Pierwsza pó³ka, rega³ A" },
        new { Code = "A-01-02", Description = "Druga pó³ka, rega³ A" },
        new { Code = "A-02-01", Description = "Trzecia pó³ka, rega³ A" },
        new { Code = "B-01-01", Description = "Pierwsza pó³ka, rega³ B" },
        new { Code = "STREFA-PRZYJ", Description = "Strefa przyjêæ – miejsce odk³adcze po przyjêciu towaru" }
    };

    foreach (var l in seedLocations)
    {
        if (!context.WarehouseLocations.Any(x => x.Code == l.Code))
        {
            context.WarehouseLocations.Add(new WarehouseLocation
            {
                Code = l.Code,
                Description = l.Description
            });
        }
    }
    await context.SaveChangesAsync();

    // --- TOWARY ---
    var seedProducts = new[]
    {
        new { Code = "TOW-0001", Name = "Paleta EURO 120x80", Unit = "szt.", Category = "Noœniki",      Desc = "Standardowa paleta 120x80" },
        new { Code = "TOW-0002", Name = "Karton 600x400x400", Unit = "szt.", Category = "Opakowania",   Desc = "Œredni karton wysy³kowy" },
        new { Code = "TOW-0003", Name = "Œruba M8x40",        Unit = "szt.", Category = "Elementy z³¹czne", Desc = "Œruba stalowa M8x40" },
        new { Code = "TOW-0004", Name = "Profil aluminiowy 2m", Unit = "szt.", Category = "Profile",   Desc = "Profil aluminiowy d³ugoœci 2m" },
        new { Code = "TOW-0005", Name = "Folia stretch 3kg",  Unit = "szt.", Category = "Materia³y pomocnicze", Desc = "Folia stretch do owijania palet" }
    };

    foreach (var p in seedProducts)
    {
        if (!context.Products.Any(x => x.Code == p.Code))
        {
            context.Products.Add(new Product
            {
                Code = p.Code,
                Name = p.Name,
                Unit = p.Unit,
                Category = p.Category,   // <=== KLUCZOWE: Category NIE JEST NULL
                Description = p.Desc,
                IsActive = true,
                Barcode = p.Code        // <=== te¿ nigdy nie jest NULL
            });
        }
    }
    await context.SaveChangesAsync();

    // --- KONTRAHENCI ---
    var seedPartners = new[]
    {
        new { Name = "Dostawca ABC Sp. z o.o.", Address = "ul. Magazynowa 1, 00-001 Warszawa", Nip = "1111111111" },
        new { Name = "Klient XYZ S.A.",       Address = "ul. Klienta 2, 00-002 Poznañ",     Nip = "2222222222" },
        new { Name = "Producent STALPOL",     Address = "ul. Przemys³owa 5, 43-300 Bielsko-Bia³a", Nip = "3333333333" }
    };

    foreach (var bp in seedPartners)
    {
        if (!context.BusinessPartners.Any(x => x.Name == bp.Name))
        {
            context.BusinessPartners.Add(new BusinessPartner
            {
                Name = bp.Name,
                Address = bp.Address,
                Nip = bp.Nip
            });
        }
    }
    await context.SaveChangesAsync();

    // --- POCZ¥TKOWE STANY MAGAZYNOWE ---
    var locA0101 = context.WarehouseLocations.FirstOrDefault(l => l.Code == "A-01-01");
    var locA0102 = context.WarehouseLocations.FirstOrDefault(l => l.Code == "A-01-02");

    var prod1 = context.Products.FirstOrDefault(p => p.Code == "TOW-0001");
    var prod2 = context.Products.FirstOrDefault(p => p.Code == "TOW-0002");
    var prod3 = context.Products.FirstOrDefault(p => p.Code == "TOW-0003");

    if (locA0101 != null && locA0102 != null && prod1 != null && prod2 != null && prod3 != null)
    {
        void EnsureStock(Product prod, WarehouseLocation loc, decimal qty)
        {
            var exists = context.StockItems.Any(s =>
                s.ProductId == prod.Id && s.WarehouseLocationId == loc.Id);

            if (!exists)
            {
                context.StockItems.Add(new StockItem
                {
                    ProductId = prod.Id,
                    WarehouseLocationId = loc.Id,
                    Quantity = qty
                });
            }
        }

        EnsureStock(prod1, locA0101, 20);
        EnsureStock(prod2, locA0101, 50);
        EnsureStock(prod3, locA0102, 100);

        await context.SaveChangesAsync();
    }
}

// ===== POTOK MIDDLEWARE =====
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // najpierw uwierzytelnianie
app.UseAuthorization();    // potem autoryzacja

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();


