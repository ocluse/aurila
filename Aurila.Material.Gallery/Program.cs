using Aurila;
using Aurila.Material;
using Aurila.Material.Gallery;
using Aurila.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAurila();
builder.Services.AddAurilaWeb();
builder.Services.AddAurilaMaterial(options =>
{
    options.Seed = "#65558F";
    options.Mode = ThemeMode.System;
});

await builder.Build().RunAsync();
