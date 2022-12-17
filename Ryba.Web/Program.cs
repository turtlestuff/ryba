using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Ryba.Data;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = new(FluentLocalizationService.DefaultLanguage);

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddSingleton<FluentLocalizationService>();
builder.Services.AddLocalization();

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
var supportedCultures = app.Services.GetRequiredService<FluentLocalizationService>().AvailableLanguages.Select(x => x.Name).ToArray();
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(FluentLocalizationService.DefaultLanguage)
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

