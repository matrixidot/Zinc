namespace Zinc.API.Interpreting;

using Builtin.Exceptions;
using Lexing;
using Parsing;

public partial class Interpreter {
    public object VisitVariableExpr(Variable expr) {
        return LookupVariable(expr.Name, expr);
    }

    public object VisitAssignExpr(Assign expr) {
        object value = Evaluate(expr.Value);
        if (locals.TryGetValue(expr, out int distance)) {
            env.AssignAt(distance, expr.Name, value);
        }
        else {
            globals.Assign(expr.Name, value);
        }
        return value;
    }
    
    public object VisitIncDecExpr(IncDec expr) {
        object value = LookupVariable(expr.Target.Name, expr.Target);
        
        if (value is not double oldValue) {
            throw new RuntimeError(expr.Op, $"Operand for {expr.Op.lexeme} must be a number.");
        }
        
        double newValue = expr.Op.type == TokenType.INCREMENT ? oldValue + 1 : oldValue - 1;

        if (locals.TryGetValue(expr.Target, out int distance)) {
            env.AssignAt(distance, expr.Target.Name, newValue);
        }
        else {
            globals.Assign(expr.Target.Name, newValue);
        }
        return expr.IsPrefix ? newValue : oldValue;
    }
}