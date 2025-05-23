using client.Components;
using client.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR.Client;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

builder.Services.AddScoped(sp =>
{
    var navMan = sp.GetRequiredService<NavigationManager>();
    return new HubConnectionBuilder()
        .WithUrl(navMan.ToAbsoluteUri("/playerhub"))
        .WithAutomaticReconnect()
        .Build();
});

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});


builder.Services.AddSingleton<GameManager>();

var app = builder.Build();

app.UseResponseCompression();
app.MapHub<TypeHub>("/typehub");
app.MapHub<LobbyHub>("/lobbyhub");
app.MapHub<PlayerHub>("/playerhub");
app.MapHub<GameHub>("/gamehub");  // Register your GameHub with a route

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
