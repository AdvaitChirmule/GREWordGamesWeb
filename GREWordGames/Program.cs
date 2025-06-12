using System.Net;
using Firebase.Auth.Providers;
using Firebase.Auth;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

var firebaseConfig = new FirebaseAuthConfig
{
    ApiKey = "AIzaSyC7924TdN6r2y43MwQs07_kZdioi3aH5Fg",
    AuthDomain = "grewordgames.firebaseapp.com",
    Providers = new FirebaseAuthProvider[]
    {
        new EmailProvider()
    }
};

builder.Services.AddSingleton(new FirebaseAuthClient(firebaseConfig));

builder.Services.AddSession();

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{action=Index}/{id?}",
    defaults: new { controller = "Home" });

app.MapControllerRoute(
    name: "default",
    pattern: "{action=Login}/{id?}",
    defaults: new { controller = "Login" });

app.Run();
