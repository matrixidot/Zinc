namespace Zinc;

using Parsing;
using Tools;

public class Zinc {
	private static bool HadError = false;
	
	public static void Main(string[] args) {
		if (args.Length > 1) {
			Console.WriteLine("Usage: zinc <path>");
			Environment.Exit(64);
		} else if (args.Length == 1) {
			RunScript(args[0]);
		}
		else {
			RunREPL();
		}
	}


	private static void RunScript(string path) {
		string lines = File.ReadAllText(path);
		Run(lines);
		
		if (HadError) Environment.Exit(65);
	}

	private static void RunREPL() {
		for (;;) {
			Console.Write("> ");
			string line = Console.ReadLine();
			if (line == null) break;
			Run(line);
			HadError = false;
		}
	}

	private static void Run(string source) {
		Scanner scanner = new Scanner(source);
		List<Token> tokens = scanner.ScanTokens();
		Parser parser = new Parser(tokens);
		Expr expression = parser.Parse();

		if (HadError) return;

		Console.WriteLine(new AstPrinter().print(expression));
	}

	public static void Error(int line, string message) {
		Report(line, "", message);
	}

	private static void Report(int line, string where, string message) {
		Console.Error.WriteLine($"[Line: {line}] Error {where} : {message}");
		HadError = true;
	}

	public static void Error(Token token, string message) {
		if (token.type == TokenType.EOF) {
			Report(token.line, ", at end", message);
		}
		else {
			Report(token.line, $" at '{token.lexeme}'", message);
		}
	}
}