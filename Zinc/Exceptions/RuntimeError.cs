namespace Zinc.Exceptions;

using Lexing;

public class RuntimeError(Token token, string message) : SystemException(message) {
    public readonly Token token = token;
}