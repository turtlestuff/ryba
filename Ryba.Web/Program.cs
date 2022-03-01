using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Ryba.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton(new FluentLocalizationService());
builder.Services.AddDbContextFactory<RybaContext>(opt => opt.UseNpgsql(
                        builder.Configuration["Ryba:ConnectionString"],
                        opt => opt.MigrationsAssembly("Ryba.Data")));
builder.Services
    .AddAuthentication(opt =>
    {
        opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddDiscord(opt =>
    {
        opt.ClientId = builder.Configuration["Ryba:ClientId"];
        opt.ClientSecret = builder.Configuration["Ryba:ClientSecret"];
        opt.ClaimActions.MapCustomJson("urn:discord:avatar:url",
            u => $"https://cdn.discordapp.com/avatars/{u.GetString("id")}/{u.GetString("avatar")}." +
            (u.GetString("avatar")?.StartsWith("a_") ?? false ? "gif" : "png"));
    });
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

