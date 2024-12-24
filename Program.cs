using Health_Hub.Data;
using Health_Hub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddDebug();
builder.Logging.AddConsole();

var connectionString = builder.Configuration.GetConnectionString("HealthHubConnectionString") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<HealthHubDbContext>(options =>options.UseSqlServer(connectionString));


builder.Services.AddSignalR().AddAzureSignalR(builder.Configuration["Azure:SignalR:ConnectionString"]);
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
	endpoints.MapHub<ChatHub>("/chathub"); // Maps SignalR hub
	endpoints.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
