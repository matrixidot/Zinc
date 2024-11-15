namespace Zinc.Lexing;

using System.Text.RegularExpressions;
using static TokenType;

public class Scanner(string Source) {
	private readonly List<Token> Tokens = [];

	private static readonly Dictionary<string, TokenType> keywords = new() {
		["if"] = IF,
		["elif"] = ELIF,
		["else"] = ELSE,
		["true"] = TRUE,
		["false"] = FALSE,
		["and"] = AND,
		["or"] = OR,
		["return"] = RETURN,
		["super"] = SUPER,
		["this"] = THIS,
		["fun"] = FUN,
		["null"] = NULL,
		["print"] = PRINT,
		["println"] = PRINTLN,
		["var"] = VAR,
		["class"] = CLASS,
		["for"] = FOR,
		["while"] = WHILE,
		["break"] = BREAK,
		["continue"] = CONTINUE,
	};
	
	private int Start = 0;
	private int Current = 0;
	private int Line = 1;

	public List<Token> ScanTokens() {
		while (!IsAtEnd()) {
			Start = Current;
			ScanToken();
		}
		Tokens.Add(new Token(EOF, "", null, Line));
		return Tokens;
	}

	private void ScanToken() {
		char c = Advance();
		switch (c) {
			case '(': AddToken(L_PAREN); break;
			case ')': AddToken(R_PAREN); break;
			case '{': AddToken(L_BRACE); break;
			case '}': AddToken(R_BRACE); break;
			case '[': AddToken(L_BRACKET); break;
			case ']': AddToken(R_BRACKET); break;
			case '~': AddToken(BITWISE_NOT); break;
			case '|': AddToken(BITWISE_OR); break;
			case '&': AddToken(BITWISE_AND); break;
			case '^': AddToken(BITWISE_XOR); break;
			case ',': AddToken(COMMA); break;
			case '.':
				if (char.IsDigit(Peek())) Number(true);
				else AddToken(DOT);
				break;
			case ';': AddToken(SEMICOLON); break;

			case '+': AddToken(Match("+") ? INCREMENT : Match("=") ? PLUS_EQUAL : PLUS); break;
			case '-': AddToken(Match("-") ? DECREMENT : Match("=") ? MINUS_EQUAL : MINUS); break;
			case '*': AddToken(Match("*=") ? POWER_EQUAL : Match("*") ? POWER : Match("=") ? MULT_EQUAL : MULT); break; 
			case '/':
				if (Match("/")) while (Peek() != '\n' && !IsAtEnd()) Advance();
				else AddToken(Match("=") ? DIV_EQUAL : DIV);
				break;

			case '%': AddToken(Match("=") ? MOD_EQUAL : MOD); break; 
			case '!': AddToken(Match("=") ? NOT_EQUAL : NOT); break;
			case '=': AddToken(Match("=") ? EQUALITY : ASSIGNMENT); break;
			case '>': AddToken(Match("=") ? GREATER_EQUAL : Match(">") ? RIGHT_SHIFT: GREATER); break;
			case '<': AddToken(Match("=") ? LESS_EQUAL : Match("<") ? LEFT_SHIFT : LESS); break;

			case '"': String(); break;

			case ' ' or '\r' or '\t': break;
			case '\n': Line++; break;

			default:
				if (char.IsDigit(c)) 
					Number(false);
				else if (char.IsLetter(c))
					Identifier();
				else
					Zinc.Error(Line, $"Unexpected Character {c}");
				break;
		}
	}

	private void Identifier() {
		while (char.IsLetterOrDigit(Peek())) Advance();
		string text = Source.Substring(Start, Current - Start);
		AddToken(keywords.GetValueOrDefault(text, IDENTIFIER), text);
	}

	private void Number(bool alreadyDecimal) {
		while (char.IsDigit(Peek())) Advance();

		if (Peek() == '.' && !alreadyDecimal) {
			if (PeekNChar(2) != '\0' && char.IsDigit(PeekNChar(2))) {
				Advance();  // Consume the decimal point
				alreadyDecimal = true;

				while (char.IsDigit(Peek())) Advance();
			} else {
				Zinc.Error(Line, "Invalid number format (trailing decimal point).");
				return;
			}
		}

		if (Peek() == '.') {
			Zinc.Error(Line, "Invalid number format (multiple decimal points).");
			return;
		}

		string numberStr = Source.Substring(Start, Current - Start);
		if (double.TryParse(numberStr, out double number)) {
			AddToken(NUMBER, number);
		} else {
			Zinc.Error(Line, $"Invalid number format: {numberStr}");
		}
	}

	private bool Match(string expected) {
		if (Current + expected.Length > Source.Length) return false;
		if (Source.Substring(Current, expected.Length) != expected) return false;

		Current += expected.Length;
		return true;
	}

	private char Peek() => IsAtEnd() ? '\0' : Source[Current];

	private char PeekNChar(int amount) {
		int targetIndex = Current + amount - 1;
		return targetIndex < Source.Length ? Source[targetIndex] : '\0';
	}

	private void String() {
		while (!IsAtEnd()) {
			if (Peek() == '"' && (Current == Start + 1 || Source[Current - 1] != '\\')) {
				break;
			}

			if (Peek() == '\n') Line++;
			Advance();
		}

		if (IsAtEnd()) {
			Zinc.Error(Line, "Unterminated string.");
			return;
		}

		Advance();
		string value = Source.Substring(Start + 1, Current - Start - 2);
		AddToken(STRING, Regex.Unescape(value));
	}

	private bool IsAtEnd() => Current >= Source.Length;

	private char Advance() => Source[Current++];
	
	private void AddToken(TokenType type, object literal = null) {
		string text = Source.Substring(Start, Current - Start);
		Tokens.Add(new Token(type, text, literal, Line));
	}
}
