using Zinc.Exceptions;
using Zinc.Parsing;
using Void = Zinc.Tools.Void;

namespace Zinc.Interpreting;

public class Interpreter : Expr.ExprVisitor<object>, Stmt.StmtVisitor<Void> {
    public void Interpret(List<Stmt> statements) {
        try {
            foreach (Stmt statement in statements) {
                Execute(statement);
            }
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
        Object left = Evaluate(expr.Left);
        Object right = Evaluate(expr.Right);

        switch (expr.Op.type) {
            case TokenType.PLUS:
                if (left is double left1 && right is double right2)
                    return left1 + right2;
                if (left is string ls) {
                    return ls + right;
                }
                throw new RuntimeError(expr.Op,
                    $"Operator {expr.Op.lexeme} is not supported for {left} and {right}");
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

    public Void VisitExpressionStmt(Expression stmt) {
        Evaluate(stmt.Expr);
        return null;
    }
    
    public Void VisitPrintStmt(Print stmt) {
        object value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    private void CheckDoubleOperands(Token op, object left, object right) {
        if (left is double && right is double) return;
        throw new RuntimeError(op, $"Operands must be numbers for operator {op.lexeme}");
    }

    private void CheckLongOperands(Token op, object left, object right, out long lleft, out long lright) {
        if (long.TryParse(left.ToString(), out long l) && long.TryParse(right.ToString(), out long r)) {
            lleft = l;
            lright = r;
            return;
        }
        throw new RuntimeError(op, $"Operands must be int types for operator {op.lexeme}");
    }
    
    private void CheckIntOperands(Token op, object left, object right, out int ileft, out int iright) {
        if (int.TryParse(left.ToString(), out int l) && int.TryParse(right.ToString(), out int r)) {
            ileft = l;
            iright = r;
            return;
        }
        throw new RuntimeError(op, $"Operands must be integers for operator {op.lexeme}");
    }
    
    private void CheckDoubleOperand(Token op, object operand) {
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

    private void Execute(Stmt stmt) {
        stmt.Accept(this);
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