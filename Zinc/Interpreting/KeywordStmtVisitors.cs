namespace Zinc.Interpreting;

using Exceptions;
using Parsing;
using Void = Tools.Void;


public partial class Interpreter {
    
    public Void VisitPrintStmt(Print stmt) {
        object value = Evaluate(stmt.Expr);
        Console.Write(Stringify(value));
        return null;
    }
    public Void VisitPrintlnStmt(Println stmt) {
        object value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }
    public Void VisitVarStmt(Var stmt) {
        object value = null;
        if (stmt.Initializer != null) {
            value = Evaluate(stmt.Initializer);
        }
        
        env.Declare(stmt.Name.lexeme, value);
        return null;
    }
    public Void VisitBreakStmt(Break stmt) {
        throw new BreakError();
    }
    public Void VisitContinueStmt(Continue stmt) {
        throw new ContinueError();
    }
    public Void VisitReturnStmt(Return stmt) {
        object value = null;
        if (stmt.Value != null) value = Evaluate(stmt.Value);
        throw new ReturnException(value);
    }
}