using Zinc.Exceptions;
using Zinc.Parsing;

namespace Zinc.Interpreting;

public class Interpreter : Visitor<object> {
    public void Interpret(Expr expression) {
        try {
            object value = Evaluate(expression);
            Console.WriteLine(Stringify(value));
        }
        catch (RuntimeError error) {
            Zinc.RuntimeError(error);
        }
    }
    
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
                CheckNumberOperand(expr.Op, right);
                return -(double)right;
            default:
                return null;
        }
    }

    public object VisitBinaryExpr(Binary expr) {
        Object left = Evaluate(expr.Left);
        Object right = Evaluate(expr.Right);

        switch (expr.Op.type) {
            case TokenType.PLUS:
                if (left is double left1 && right is double right2)
                    return left1 + right2;
                if (left is string ls && right is string rs)
                    return $"{ls}{rs}";
                if (left is string s2 && right is double d2)
                    return s2 + d2;
                throw new RuntimeError(expr.Op,
                    $"Operator {expr.Op.lexeme} is not supported for {left} and {right}");

                break;
            case TokenType.MINUS:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left - (double)right;
            case TokenType.DIV:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left / (double)right;
            case TokenType.MULT:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left * (double)right;
            case TokenType.MOD:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left % (double)right;
            case TokenType.POWER:
                CheckNumberOperands(expr.Op, left, right);
                return Math.Pow((double)left, (double)right);
            case TokenType.PLUS_EQUAL:
                if (left is double d && right is double right3) {
                    left = d + right3;
                    return left;
                }

                if (left is string s && right is string right4) {
                    left = s + right4;
                    return left;
                }

                if (left is string s1 && right is double d1) {
                    left = s1 + d1;
                    return left;
                }

                throw new RuntimeError(expr.Op,
                    $"Operator {expr.Op.lexeme} is not supported for {left} and {right}");
            case TokenType.MINUS_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                left = (double)left - (double)right;
                return left;            
            case TokenType.MULT_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                left = (double)left * (double)right;
                return left;            
            case TokenType.DIV_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                left = (double)left / (double)right;
                return left;            
            case TokenType.MOD_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                left = (double)left % (double)right;
                return left;            
            case TokenType.POWER_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                left = Math.Pow((double)left, (double)right);
                return left;
            case TokenType.GREATER:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left <= (double)right;
            case TokenType.EQUALITY:
                return left == right;
            case TokenType.NOT_EQUAL:
                return left != right;
        }

        return null;
    }

    private void CheckNumberOperands(Token op, object left, object right) {
        if (left is double && right is double) return;
        throw new RuntimeError(op, $"Operands must be numbers for operator {op.lexeme}");
    }
    
    private void CheckNumberOperand(Token op, object operand) {
        if (operand is double) return;
        throw new RuntimeError(op, $"Operand must be a number for operator type {op.lexeme}");
    }
    
    private bool IsTruthy(object obj) {
        Console.WriteLine(obj);
        return obj switch {
            null => false,
            bool b => b,
            _ => true
        };
    }
    
    private object Evaluate(Expr expr) {
        return expr.Accept(this);
    }

    private string Stringify(object obj) {
        if (obj == null) return "null";

        if (obj is double) {
            string text = obj.ToString();
            if (text.EndsWith(".0")) {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }

        return obj.ToString();
    }
}