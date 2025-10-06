using OllamaChatClient.Console.Models;

namespace OllamaChatClient.Console.UI;
using Console = System.Console;

public class ModelSelector
{
    // private readonly IOllamaService _ollama;
    // private readonly ChatInterface _chatInterface;
    //
    // public ModelSelector(IOllamaService ollama, ChatInterface chatInterface)
    // {
    //     _ollama = ollama;
    //     _chatInterface = chatInterface;
    // }

    private readonly List<OllamaModel> _models = new()
    {
        new OllamaModel { Name = "Model 1" },
        new OllamaModel { Name = "Model 2" },
        new OllamaModel { Name = "Model 3" },
    };

    public async Task RunAsync()
    {
        while (true)
        {
            Console.CursorVisible = false;
            Console.Clear();

            // App name in white
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("OLLAMA CHAT\n");
            Console.ResetColor();

            // Header in subtle gray
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Select model to chat with:\n");
            Console.ResetColor();

            //var models = await _ollama.GetModelsAsync();
            var models = _models;
            if (models.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("No models found. Make sure Ollama is running in Docker.\n");
                Console.ResetColor();
                Console.WriteLine("Press any key to retry...");
                Console.ReadKey(true);
                continue;
            }

            var options = models.Select(m => m.Name).ToList();
            options.Add("Exit");

            int index = 0;
            ConsoleKey key;
            
            do
            {
                Console.Clear();

                // App name and header
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("OLLAMA CHAT\n");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Select model to chat with:\n");
                Console.ResetColor();
                
                // Menu items
                for (int i = 0; i < options.Count; i++)
                {
                    bool isExit = i == options.Count - 1;

                    string text = options[i];

                    if (i == index)
                    {
                        // Highlight only text, not extra line space
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                        Console.ForegroundColor = isExit ? ConsoleColor.White : ConsoleColor.Black;
                        Console.WriteLine(isExit ? $"\n[x] {text}" : $"[x] {text}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = isExit ? ConsoleColor.DarkRed : ConsoleColor.DarkGray;
                        Console.WriteLine(isExit ? $"\n[ ] {text}" : $"[ ] {text}");
                        Console.ResetColor();
                    }
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    if (index > 0) index--; // cannot go above first
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    if (index < options.Count - 1) index++; // cannot go below last
                }

            } while (key != ConsoleKey.Enter);

            var selected = options[index];
            if (selected.Equals("Exit", StringComparison.OrdinalIgnoreCase))
                break;

            await new ChatWindow().StartChatAsync(selected);
        }
    }
}