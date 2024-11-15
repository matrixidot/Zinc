namespace Zinc.API.Interpreting;

using Builtin.Exceptions;
using Builtin.Functions;
using Parsing;
using Tools;
using Void = Utils.Void;

public partial class Interpreter : ExprVisitor<object>, StmtVisitor<Void> {
    private readonly Environment globals;
    private readonly Dictionary<Expr, int> locals = new();
    private Environment env;
    private bool suppressOutput = false;

    public Interpreter() {
        globals = new Environment();
        env = globals;
        
        InitializeGlobals();
    }
    
    private void InitializeGlobals() {
        globals.Define("clock", new FuncBIClock());
        globals.Define("print", new FuncBIPrint());
        globals.Define("println", new FuncBIPrintln());
        // Add any other built-in functions/variables here
    }
    public void Interpret(List<Stmt> statements) {
        try {
            // Special case for single expression statements in REPL
            if (statements.Count == 1 && statements[0] is Expression expr) {
                object value = Evaluate(expr.Expr);
                if (!suppressOutput && value is not Void) {
                    Console.WriteLine(Stringify(value));
                }
                return;
            }
            
            foreach (Stmt statement in statements) {
                Execute(statement);
            }
        }
        catch (RuntimeError error) {
            Zinc.RuntimeErrored(error);
        }
        catch (ReturnException returnValue) {
            // Handle top-level returns gracefully
            if (!suppressOutput && returnValue.Value != null) {
                Console.WriteLine(Stringify(returnValue.Value));
            }
        }
    }
}