namespace Zinc.API.Utils;

using Interpreting;

public class ZincClass(string name) : ZincCallable {
    public string Name { get; } = name;

    
    public object Call(Interpreter interpreter, List<object> arguments) {
        ZincInstance instance = new ZincInstance(this);
        return instance;
    }
    
    public int Arity() => 0;

    public override string ToString() => Name;
}