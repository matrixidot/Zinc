namespace Zinc.Interpreting;

using Exceptions;
using Lexing;
using Parsing;

public partial class Interpreter {
    public object VisitVariableExpr(Variable expr) {
        return env.Get(expr.Name);
    }

    public object VisitAssignExpr(Assign expr) {
        object value = Evaluate(expr.Value);
        env.Assign(expr.Name, value);
        return value;
    }
    
    public object VisitIncDecExpr(IncDec expr) {
        object value = env.Get(expr.Target.Name);
        
        if (value is not double oldValue) {
            throw new RuntimeError(expr.Op, $"Operand for {expr.Op.lexeme} must be a number.");
        }
        
        double newValue = expr.Op.type == TokenType.INCREMENT ? oldValue + 1 : oldValue - 1;
        env.Assign(expr.Target.Name, newValue);
        return expr.IsPrefix ? newValue : oldValue;
    }
}