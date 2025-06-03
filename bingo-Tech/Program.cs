using bingo_Tech.Hubs;
using bingo_Tech.IServices;
using bingo_Tech.Models;
using bingo_Tech.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<CallLogContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
        "Server=LAPTOP-K8QC50ME;Database=VoiceChatApp;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;"));

builder.Services.AddScoped<ICallLogService, CallLogService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.UseRouting();

app.MapRazorPages();
app.MapHub<VoiceChatHub>("/voicechatHub");
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CallLogContext>();
    context.Database.EnsureCreated();
}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
