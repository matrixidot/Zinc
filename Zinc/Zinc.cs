namespace Zinc;

using Exceptions;
using Interpreting;
using Lexing;
using Parsing;
using static Environment;

public class Zinc {
	private static readonly Interpreter interpreter = new Interpreter();
	private static bool HadError = false;
	private static bool HadRuntimeError = false;
	
	public static void Main(string[] args) {
		Console.Write("Enter path to run script or hit enter for REPL: ");
		string str = Console.ReadLine();
		if (Path.Exists(str)) {
			RunScript(str);
			Console.Write("Exit? (y)");
			str = Console.ReadLine();
			if (str == "y")
				Exit(64);
			else
				Main(args);
		}
		else {
			RunREPL();
		}
	}
	

	private static void RunScript(string path) {
		string lines = File.ReadAllText(path);
		Run(lines);
		
		if (HadError) Exit(65);
		if (HadRuntimeError) Exit(70);
	}

	private static void RunREPL() {
		for (;;) {
			Console.Write(">>> ");
			string line = Console.ReadLine();
			if (line == null) break;
			if (line.Trim().EndsWith('{')) {
				// endingSequence = line.Trim().Last() == '{' ? "}" : ");";
				string endingSequence = "}";
				int numEndings = 1;

				while (true) {
					if (line.Trim().EndsWith(endingSequence)) {
						numEndings--;
						if (numEndings == 0) break;
					}
					for (int i = 0; i < numEndings; i++) {
						Console.Write("...");
					}
					Console.Write(" ");
					line += Console.ReadLine();
					if (line.Trim().EndsWith('{')) {
						numEndings++;
					}
				}
			}
			Run(line);
			HadError = false;
		}
	}

	private static void Run(string source) {
		Scanner scanner = new Scanner(source);
		List<Token> tokens = scanner.ScanTokens();
		Parser parser = new Parser(tokens);
		List<Stmt> statements = parser.Parse();
		
		if (HadError) return;

		interpreter.Interpret(statements);
	}

	public static void Error(int line, string message) {
		Report(line, "", message);
	}

	private static void Report(int line, string where, string message) {
		Console.Error.WriteLine($"[Line: {line}] Error {where}: {message}");
		HadError = true;
	}

	public static void Error(Token token, string message) {
		if (token.type == TokenType.EOF) {
			Report(token.line, "at end", message);
		}
		else {
			Report(token.line, $"at '{token.lexeme}'", message);
		}
	}

	public static void RuntimeError(RuntimeError error) {
		Console.Error.WriteLine($"{error.Message}\n{{line {error.token.line}}}");
		HadRuntimeError = true;
	}
}