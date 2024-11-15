namespace Zinc.Lexing;

public record Token(TokenType type, string lexeme, object literal, int line) {
	
	
	public override string ToString() {
		return $"[Type:{type}, Text:{lexeme}, Value:{literal}]";
	}
}