namespace Zinc.API.Builtin.Functions;

using Interpreting;
using Utils;

public class FuncBIClock : ZincCallable {
    public int Arity() => 0;

    public object Call(Interpreter interpreter, List<object> arguments) => (double) DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond / 1000.0;

    public override string ToString() => "<native function>";
}