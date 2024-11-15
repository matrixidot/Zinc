namespace Zinc.BuiltinFunctions;

using Exceptions;
using Interpreting;
using Parsing;
using Tools;

public class ZincFunction(Function declaration, Environment closure) : ZincCallable {
    private Function Declaration { get; } = declaration;

    private Environment Closure { get; } = closure;

    public object Call(Interpreter interpreter, List<object> arguments) {
        Environment env = new Environment(Closure);
        for (int i = 0; i < declaration.Parameters.Count; i++) {
            env.Declare(declaration.Parameters[i].lexeme, arguments[i]);
        }
        try {
            interpreter.ExecuteBlock(declaration.Body, env);
        }
        catch (ReturnException e) {
            return e.Value;
        }
        return null;
    }

    public int Arity() => declaration.Parameters.Count;

    public override string ToString() => $"<function {declaration.Name.lexeme}>";
}