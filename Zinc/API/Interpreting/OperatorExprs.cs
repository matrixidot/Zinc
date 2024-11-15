namespace Zinc.API.Interpreting;

using Builtin.Exceptions;
using Lexing;
using Parsing;

public partial class Interpreter {
    public object VisitLiteralExpr(Literal expr) {
        return expr.Value;
    }

    public object VisitGroupingExpr(Grouping expr) {
        return Evaluate(expr.Expression);
    }

    public object VisitUnaryExpr(Unary expr) {
        object right = Evaluate(expr.Right);

        switch (expr.Op.type) {
            case TokenType.NOT:
                return !IsTruthy(right);
            case TokenType.MINUS:
                CheckDoubleOperand(expr.Op, right);
                return -(double)right;
            case TokenType.BITWISE_NOT:
                if (!long.TryParse(right.ToString(), out long l)) throw new RuntimeError(expr.Op, $"Operand is not a long for operator {expr.Op.lexeme}");
                return ~l;
            default:
                return null;
        }
    }

    public object VisitBinaryExpr(Binary expr) {
        object left = Evaluate(expr.Left);
        object right = Evaluate(expr.Right);

        switch (expr.Op.type) {
            case TokenType.PLUS:
                return left switch {
                    double left1 when right is double right2 => left1 + right2,
                    string ls => ls + right,
                    _ => throw new RuntimeError(expr.Op, $"Operator {expr.Op.lexeme} is not supported for {left} and {right}")
                };
            case TokenType.MINUS:
                CheckDoubleOperands(expr.Op, left, right);
                return (double)left - (double)right;
            case TokenType.DIV:
                CheckDoubleOperands(expr.Op, left, right);
                return (double)left / (double)right;
            case TokenType.MULT:
                CheckDoubleOperands(expr.Op, left, right);
                return (double)left * (double)right;
            case TokenType.MOD:
                CheckDoubleOperands(expr.Op, left, right);
                return (double)left % (double)right;
            case TokenType.POWER:
                CheckDoubleOperands(expr.Op, left, right);
                return Math.Pow((double)left, (double)right);
            case TokenType.GREATER:
                CheckDoubleOperands(expr.Op, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckDoubleOperands(expr.Op, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckDoubleOperands(expr.Op, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckDoubleOperands(expr.Op, left, right);
                return (double)left <= (double)right;
            case TokenType.EQUALITY:
                return left.Equals(right);
            case TokenType.NOT_EQUAL:
                return !left.Equals(right);
            case TokenType.BITWISE_OR:
                CheckLongOperands(expr.Op, left, right, out long lleft, out long lright);
                return lleft | lright;
            case TokenType.BITWISE_XOR:
                CheckLongOperands(expr.Op, left, right, out long lleft1, out long lright1);
                return lleft1 ^ lright1;
            case TokenType.BITWISE_AND:
                CheckLongOperands(expr.Op, left, right, out long lleft2, out long lright2);
                return lleft2 & lright2;
            case TokenType.LEFT_SHIFT:
                CheckIntOperands(expr.Op, left, right, out int ileft, out int iright);
                return ileft << iright;
            case TokenType.RIGHT_SHIFT:
                CheckIntOperands(expr.Op, left, right, out int ileft1, out int iright1);
                return ileft1 >> iright1;
        }

        return null;
    }
}