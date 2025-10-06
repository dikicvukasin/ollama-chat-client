// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaChatClient.Console.UI;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // services.AddHttpClient<IOllamaService, OllamaService>(client =>
        // {
        //     client.BaseAddress = new Uri("http://localhost:11434/api/");
        // });
        services.AddSingleton<ModelSelectorWindow>();
        services.AddSingleton<ChatWindow>();
    })
    .Build();

var menu = host.Services.GetRequiredService<ModelSelectorWindow>();
await menu.RunAsync();