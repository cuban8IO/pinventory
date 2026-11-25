using Microsoft.EntityFrameworkCore;
using Pinventory.Components;
using Pinventory.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<PinventoryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Pinventory")));

var app = builder.Build();

// Datenbank anlegen bzw. auf den neuesten Stand bringen.
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PinventoryDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
