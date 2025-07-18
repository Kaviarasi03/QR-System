
using Login.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// **✅ Configure Database Context**
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// **✅ Configure Identity**
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;         // At least one number
    options.Password.RequiredLength = 6;          // Minimum length of 6
    options.Password.RequireNonAlphanumeric = true; // At least one special character
    options.Password.RequireUppercase = true;     // At least one uppercase letter
    options.Password.RequireLowercase = false;    // Lowercase is optional
    options.Password.RequiredUniqueChars = 1;     // At least one unique character
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();



var app = builder.Build();

// **✅ Seed Default Admin User and Roles**
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.InitializeAsync(userManager, roleManager);
}

// **✅ Configure Middleware**
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // **✅ Fix: Authentication should come before Authorization**
app.UseAuthorization();

// **✅ Route Configuration**
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Dashboard}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "employee",
    pattern: "Employee/{action=Dashboard}",
    defaults: new { controller = "Employee" });

app.Run();
