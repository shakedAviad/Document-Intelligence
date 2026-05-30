using DocumentIntelligence.Console;
using Microsoft.Extensions.Hosting;

await Host.CreateApplicationBuilder(args)
    .ConfigureServices()
    .BuildApplication()
    .RunAsync();
