using Docusign.Interface;
using Docusign.Services;
using RazorLight;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();
builder.Services.AddTransient<IDocumentService, DocumentService>();
builder.Services.AddScoped<IRazorLightEngine>(provider =>
{
    return new RazorLightEngineBuilder()
        .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory())) // Adjust the path to your Views folder
        .UseMemoryCachingProvider()
        .Build();
});
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=FirstPage}/");

app.Run();
