using AutoDrive.Data;
using AutoDrive.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AutoDriveContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// IDENTITY
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AutoDriveContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

var app = builder.Build();


// ================= SEED =================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AutoDriveContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    context.Database.EnsureCreated();

    // GENDERS
    if (!context.genders.Any())
    {
        context.genders.AddRange(
            new Gender { Name = "Мъж" },
            new Gender { Name = "Жена" }
        );
        context.SaveChanges();
    }

    // COUNTRIES
    if (!context.countries.Any())
    {
        context.countries.AddRange(
            new Country { Name = "България" },
            new Country { Name = "Германия" },
            new Country { Name = "Франция" },
            new Country { Name = "Италия" },
            new Country { Name = "Испания" },
            new Country { Name = "Гърция" },
            new Country { Name = "Румъния" },
            new Country { Name = "Турция" },
            new Country { Name = "Великобритания" },
            new Country { Name = "САЩ" }
        );
        context.SaveChanges();
    }

    // ROLES (SAFE VERSION)
    string[] roles = { "Admin", "Trainer", "Student" };

    foreach (var role in roles)
    {
        var exists = roleManager.RoleExistsAsync(role).GetAwaiter().GetResult();

        if (!exists)
        {
            roleManager.CreateAsync(new IdentityRole(role))
                        .GetAwaiter()
                        .GetResult();
        }
    }
}


// ================= MIDDLEWARE =================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();