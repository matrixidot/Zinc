namespace Zinc;

using System.Diagnostics;
using System.Text;
using API.Builtin.Exceptions;
using API.Interpreting;
using API.Lexing;
using API.Parsing;
using API.Resolving;
using API.Utils;
using Tools;
using Environment = System.Environment;

public class Zinc {
    private static Interpreter interpreter = new Interpreter();
    private static Resolver resolver = new Resolver(interpreter);
    private static bool HadError;
    private static bool HadRuntimeError;
    
    public static void Main(string[] args) {
        if (args.Length != 0) {
            RunScript(args[0]);
        }
        else {
            ShowWelcomeMessage();
            RunInteractiveMode(); 
        }
    }

    private static void ShowWelcomeMessage() {
        Console.WriteLine("Zinc Programming Language REPL");
        Console.WriteLine("Type '.help' for commands, '.exit' to quit");
        Console.WriteLine("----------------------------------------");
    }

    private static void RunInteractiveMode() {
        while (true) {
            Console.Write(">>> ");
            string input = Console.ReadLine();
            
            if (string.IsNullOrEmpty(input)) continue;

            // Handle REPL commands
            if (input.StartsWith(".")) {
                HandleReplCommand(input);
                continue;
            }

            // Check for unclosed brackets
            if (HasUnclosedBrackets(input)) {
                input = HandleMultilineInput(input);
            }

            try {
                RunAndResetErrors(input);
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void HandleReplCommand(string command) {
        string[] parts = command.ToLower().Split(' ');
        string cmd = parts[0];
        
        switch (cmd) {
            case ".help":
                ShowHelp();
                break;
            case ".exit":
                Environment.Exit(0);
                break;
            case ".clear":
                Console.Clear();
                ShowWelcomeMessage();
                break;
            case ".new":
                interpreter = new Interpreter();
                resolver = new Resolver(interpreter);
                HadRuntimeError = false;
                HadError = false;
                Console.Clear();
                Console.WriteLine("========================");
                Console.WriteLine("Interpreter state reset.");
                Console.WriteLine("========================");
                ShowWelcomeMessage();
                break;
            case ".run":
                if (parts.Length < 2) {
                    Console.WriteLine("Error: Please provide a script path");
                    return;
                }
                string path = string.Join(" ", parts.Skip(1));
                RunScriptInNewWindow(path);
                break;
            default:
                Console.WriteLine($"Unknown command: {command}");
                Console.WriteLine("Type '.help' for available commands");
                break;
        }
    }

    private static void ShowHelp() {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  .help       - Show this help message");
        Console.WriteLine("  .exit       - Exit the REPL");
        Console.WriteLine("  .clear      - Clear the screen");
        Console.WriteLine("  .new        - Starts a fresh interpreter session");
        Console.WriteLine("  .run <path> - Run script in new window");
        Console.WriteLine("\nMulti-line input:");
        Console.WriteLine("  Unclosed brackets (, [, { will enter multi-line mode");
        Console.WriteLine("  Close all brackets to execute the code");
    }

    private static bool HasUnclosedBrackets(string input) {
        int roundBrackets = 0;
        int squareBrackets = 0;
        int curlyBrackets = 0;

        foreach (char c in input) {
            switch (c) {
                case '(': roundBrackets++; break;
                case ')': roundBrackets--; break;
                case '[': squareBrackets++; break;
                case ']': squareBrackets--; break;
                case '{': curlyBrackets++; break;
                case '}': curlyBrackets--; break;
            }
        }

        return roundBrackets > 0 || squareBrackets > 0 || curlyBrackets > 0;
    }

    private static string HandleMultilineInput(string input) {
        int roundBrackets = 0;
        int squareBrackets = 0;
        int curlyBrackets = 0;
        
        StringBuilder code = new StringBuilder(input);
        code.AppendLine();

        // Count initial brackets
        foreach (char c in input) {
            UpdateBracketCount(c, ref roundBrackets, ref squareBrackets, ref curlyBrackets);
        }

        while (roundBrackets > 0 || squareBrackets > 0 || curlyBrackets > 0) {
            int totalBrackets = roundBrackets + squareBrackets + curlyBrackets;
            Console.Write("... ".PadLeft(totalBrackets + 4));
            string line = Console.ReadLine();
            
            if (string.IsNullOrEmpty(line)) continue;
            
            code.AppendLine(line);
            
            foreach (char c in line) {
                UpdateBracketCount(c, ref roundBrackets, ref squareBrackets, ref curlyBrackets);
            }
        }

        return code.ToString();
    }

    private static void UpdateBracketCount(char c, ref int round, ref int square, ref int curly) {
        switch (c) {
            case '(': round++; break;
            case ')': round--; break;
            case '[': square++; break;
            case ']': square--; break;
            case '{': curly++; break;
            case '}': curly--; break;
        }
    }

    private static void RunScript(string path) {
        string lines = File.ReadAllText(path);
        Run(lines);

        if (HadError) {
            Console.WriteLine("Errored while running script, press any key to continue");
            Console.ReadKey();
            Environment.Exit(65);
        }
        if (HadRuntimeError) {
            Console.WriteLine("Errored while running script, press any key to continue");
            Console.ReadKey();
            Environment.Exit(70);
        }

        
        Console.WriteLine("\n\nFinished, press any key to exit...");
        Console.ReadKey();
        Environment.Exit(0);
    }
    
    private static void RunScriptInNewWindow(string path) {
        if (!File.Exists(path)) {
            Console.WriteLine($"Error: Could not find file '{path}'");
            return;
        }

        try {
            // Start a new process with the current executable and the script path
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = Environment.ProcessPath;
            startInfo.Arguments = $"\"{path}\"";
            startInfo.UseShellExecute = true;  // This opens in a new window
            
            Process.Start(startInfo);
        }
        catch (Exception ex) {
            Console.WriteLine($"Error launching script: {ex.Message}");
        }
    }

    private static void Run(string source) {
        Scanner scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();
        Parser parser = new Parser(tokens);
        List<Stmt> statements = parser.Parse();
		
        if (HadError) return;
        
        resolver.Resolve(statements);

        if (HadError) return;
        
        interpreter.Interpret(statements);
    }
    
    private static void RunAndResetErrors(string source) {
        Run(source);
        HadError = false;
        HadRuntimeError = false;
    }

    public static void Error(int line, string message) {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message) {
        Console.Error.WriteLine($"[Line {line}] Error{(where.Length > 0 ? $" {where}" : "")}: {message}");
        HadError = true;
    }

    public static void Error(Token token, string message) {
        if (token.type == TokenType.EOF) {
            Report(token.line, "at end", message);
        } else {
            Report(token.line, $"at '{token.lexeme}'", message);
        }
    }

    public static void RuntimeErrored(RuntimeError error) {
        Console.Error.WriteLine($"{error.Message}\n[Line {error.token.line}]");
        HadRuntimeError = true;
    }
}