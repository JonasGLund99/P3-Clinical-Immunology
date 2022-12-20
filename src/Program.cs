using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using src.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.MaximumReceiveMessageSize = 10000000;
});

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
if (args.Contains("testmode"))
{
    DatabaseService.EnableTestMode();
}
await DatabaseService.Instance.SetupDatabase();

app.Run();
