using OllamaChatClient.Console.Services;

namespace OllamaChatClient.Console.UI;
using Console = System.Console;

public class ChatWindow
{
    private readonly IOllamaClient _ollama;
    
    public ChatWindow(IOllamaClient ollama)
    {
        _ollama = ollama;
    }

    public async Task StartChatAsync(string model)
    {
        Console.CursorVisible = true; // we want user input visible here
        ClearAndDrawHeader(model);

        while (true)
        {
            // User input
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("You: ");
            Console.ResetColor();

            string? input = Console.ReadLine();
            if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                break;
            
            if (string.Equals(input, "clear", StringComparison.OrdinalIgnoreCase))
            {
                ClearAndDrawHeader(model);
                continue;
            }

            if (string.IsNullOrWhiteSpace(input))
                continue;

            // Ollama response
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nOllama:");
            Console.ResetColor();

            try
            {
                // Stream the response chunk by chunk
                await foreach (var (chunk, isThinking) in _ollama.StreamMessageAsync(model, input!))
                {
                    if (!isThinking)
                        Console.ForegroundColor = ConsoleColor.White; // final response
                    else
                        Console.ForegroundColor = ConsoleColor.DarkGray; // thinking

                    Console.Write(chunk);
                    Console.ResetColor();
                } 

                Console.WriteLine("\n"); // spacing after message
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError: {ex.Message}\n");
                Console.ResetColor();
            }

            // Optional: small separator for readability
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('-', Console.WindowWidth));
            Console.ResetColor();
        }

        Console.CursorVisible = false; // hide again when returning to menu
    }
    
    private void ClearAndDrawHeader(string model)
    {
        Console.Write("\x1b[3J\x1b[H\x1b[2J");

        // Header
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("OLLAMA CHAT - Model: " + model + "\n");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Type 'exit' to return to model selection.");
        Console.WriteLine("Type 'clear' to clear the chat.\n");
        Console.ResetColor();
    }
}

