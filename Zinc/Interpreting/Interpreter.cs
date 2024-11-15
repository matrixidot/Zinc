namespace Zinc.Interpreting;

using BuiltinFunctions;
using Exceptions;
using Lexing;
using Parsing;
using Tools;
using Void = Tools.Void;

public partial class Interpreter : Expr.ExprVisitor<object>, Stmt.StmtVisitor<Void> {
    public readonly Environment globals;
    private Environment env;
    public Interpreter() {
        globals = new Environment();
        env = globals;
        
        globals.Declare("clock", new Clock());
    }
    
    
    private bool suppressOutput = false;
    public void Interpret(List<Stmt> statements) {
        try {
            if (statements.Count == 1 && statements[0] is Expression expr) {
                object value = Evaluate(expr.Expr);
                if (value == null) Console.WriteLine();
                else Console.WriteLine(Stringify(value));
                return;
            }
            
            foreach (Stmt statement in statements) {
                Execute(statement);
            }
        }
        catch (RuntimeError error) {
            Zinc.RuntimeError(error);
        }
    }
}