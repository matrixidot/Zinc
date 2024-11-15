namespace Zinc.API.Interpreting;

using Builtin.Exceptions;
using Parsing;
using Void = Utils.Void;


public partial class Interpreter {
    public Void VisitVarStmt(Var stmt) {
        object value = null;
        if (stmt.Initializer != null) {
            value = Evaluate(stmt.Initializer);
        }
        
        env.Define(stmt.Name.lexeme, value);
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