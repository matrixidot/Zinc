using Tools;

public static class Program {
    private static readonly string[] toolArgs = ["GenAst"];
    private static string previousPath = string.Empty;
    private static string path = @"C:\Users\Romir\Desktop\Projects\C#\Zinc\Zinc\Parsing";

    public static void Main(string[] args) {
        while (true) {
            DisplayToolOptions();
            int tool = GetToolSelection();

            if (path == string.Empty || !Path.Exists(path))
                path = GetPath();
            if (path.Equals("quit", StringComparison.OrdinalIgnoreCase) || path.Equals("exit", StringComparison.OrdinalIgnoreCase)) {
                Console.WriteLine("Exiting program.");
                break;
            }

            ExecuteTool(tool, path);
            Console.WriteLine("\n--- Operation completed. Type 'exit' or 'quit' to stop, or continue to select a new tool ---\n");
        }
    }

    private static void ExecuteTool(int tool, string path) {
        switch (toolArgs[tool]) {
            case "GenAst":
                DefineAsts.Run(path);
                break;
            default:
                Console.Error.WriteLine("Tool not implemented.");
                Environment.Exit(64);
                break;
        }
    }
    
    private static void DisplayToolOptions() {
        Console.WriteLine("Available Tools:");
        for (int i = 0; i < toolArgs.Length; i++) {
            Console.WriteLine($"{i}) {toolArgs[i]}");
        }
    }

    private static int GetToolSelection() {
        Console.Write("Select tool number: ");
        string s = Console.ReadLine();
        if (s.Equals("quit", StringComparison.OrdinalIgnoreCase) || s.Equals("exit", StringComparison.OrdinalIgnoreCase)) {
            Console.WriteLine("Exiting program.");
            Environment.Exit(64);
        }
        if (int.TryParse(s, out int tool) && tool >= 0 && tool < toolArgs.Length) return tool;
        Console.Error.WriteLine("Invalid tool selection.");
        Console.Error.WriteLine("Valid tool numbers are:");
        DisplayToolOptions();
        Environment.Exit(64);
        return tool;
    }

    private static string GetPath() {
        while (true) {
            Console.Write("Enter path (or type 'prevPath' to use the previous path, 'exit' to quit): ");
            string path = Console.ReadLine();

            if (path.Equals("prevPath", StringComparison.OrdinalIgnoreCase)) {
                if (string.IsNullOrEmpty(previousPath)) {
                    Console.Error.WriteLine("No previous path found.");
                    continue;
                }
                path = previousPath;
                Console.WriteLine($"Using previous path: {path}");
            }

            if (string.IsNullOrWhiteSpace(path) || path.Equals("exit", StringComparison.OrdinalIgnoreCase) || path.Equals("quit", StringComparison.OrdinalIgnoreCase)) {
                Console.WriteLine("Exiting program.");
                Environment.Exit(64);
            }

            previousPath = path; // Store the path for future reference
            return path;
        }
    }
}

