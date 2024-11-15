namespace Zinc.API.Interpreting;

using Builtin.Exceptions;
using Lexing;
using Parsing;
using Utils;

public partial class Interpreter {
    public object VisitLogicalExpr(Logical expr) {
        object left = Evaluate(expr.Left);

        if (expr.Op.type == TokenType.OR) {
            if (IsTruthy(left)) return left;
        }
        else {
            if (!IsTruthy(left)) return left;
        }
        
        return Evaluate(expr.Right);
    }

    public object VisitCallExpr(Call expr) {
        object callee = Evaluate(expr.Callee);

        List<object> arguments = expr.Arguments.Select(Evaluate).ToList();

        if (callee is not ZincCallable function) {
            throw new RuntimeError(expr.Paren, "Not a valid function/class call.");
        }

        if (arguments.Count != function.Arity()) {
            throw new RuntimeError(expr.Paren, $"Expected {function.Arity()} arguments but got {arguments.Count} instead.");
        }
        
        return function.Call(this, arguments);
    }


}