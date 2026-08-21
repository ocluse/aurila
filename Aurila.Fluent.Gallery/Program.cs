using Aurila;
using Aurila.Fluent;
using Aurila.Fluent.Gallery;
using Aurila.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAurila();
builder.Services.AddAurilaWeb();
builder.Services.AddAurilaFluent(options =>
{
    options.Seed = "#0F6CBD";
    options.Mode = FluentThemeMode.System;
});

await builder.Build().RunAsync();
