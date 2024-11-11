namespace Zinc.Exceptions;

public class RuntimeError(Token token, string message) : SystemException(message) {
    public readonly Token token = token;
}