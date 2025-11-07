using Counter.Components;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using RazorConsole;
using RazorConsole.Core;

ExcelPackage.License.SetNonCommercialPersonal("Delcoigne Vincent");

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
    .UseRazorConsole<Counter2>();
IHost host = hostBuilder.Build();
await host.RunAsync();
