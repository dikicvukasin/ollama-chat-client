// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OllamaChatClient.Console.Services;
using OllamaChatClient.Console.UI;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
    })
    .ConfigureServices((_, services) =>
    {
        services.AddHttpClient<IOllamaClient, OllamaClient>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:11435/api/");
        });
        services.AddSingleton<ModelSelectorWindow>();
        services.AddSingleton<ChatWindow>();
    })
    .Build();

var menu = host.Services.GetRequiredService<ModelSelectorWindow>();
await menu.RunAsync();