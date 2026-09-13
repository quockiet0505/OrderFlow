using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OrderFlow.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var ordersUrl = builder.Configuration["ApiBaseUrls:OrdersApi"];
var inventoryUrl = builder.Configuration["ApiBaseUrls:InventoryApi"];

builder.Services.AddHttpClient("OrdersApi", client => client.BaseAddress = new Uri(ordersUrl ?? "http://localhost:5184"));
builder.Services.AddHttpClient("InventoryApi", client => client.BaseAddress = new Uri(inventoryUrl ?? "http://localhost:5146"));

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("OrdersApi"));
await builder.Build().RunAsync();
