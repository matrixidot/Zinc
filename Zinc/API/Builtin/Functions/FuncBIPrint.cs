namespace Zinc.API.Builtin.Functions;

using Interpreting;
using Utils;

public class FuncBIPrint : ZincCallable {
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> arguments) {
        Console.Write(interpreter.Stringify(arguments[0]));
        return new Void();
    }

    public override string ToString() => "<native function>";
}