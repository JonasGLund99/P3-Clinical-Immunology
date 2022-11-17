using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using src.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Setup database
await DatabaseService.Instance.SetupDatabase();
await Mocker.Mock(DatabaseService.Instance.Database);

app.Run();
