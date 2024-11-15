namespace Zinc.Interpreting;

using Exceptions;
using Lexing;
using Parsing;
using Tools;

public partial class Interpreter {
    private void CheckDoubleOperands(Token op, object left, object right) {
        if (left is double && right is double) return;
        throw new RuntimeError(op, $"Operands must be numbers for operator {op.lexeme}");
    }

    private void CheckLongOperands(Token op, object left, object right, out long lleft, out long lright) {
        if (!long.TryParse(left.ToString(), out long l) || !long.TryParse(right.ToString(), out long r)) throw new RuntimeError(op, $"Operands must be int types for operator {op.lexeme}");
        lleft = l;
        lright = r;
    }
    
    private void CheckIntOperands(Token op, object left, object right, out int ileft, out int iright) {
        if (!int.TryParse(left.ToString(), out int l) || !int.TryParse(right.ToString(), out int r)) throw new RuntimeError(op, $"Operands must be integers for operator {op.lexeme}");
        ileft = l;
        iright = r;
    }
    
    private void CheckDoubleOperand(Token op, object operand) {
        if (operand is double) return;
        throw new RuntimeError(op, $"Operand must be a number for operator type {op.lexeme}");
    }
    
    private bool IsTruthy(object obj) {
        return obj switch {
            null => false,
            bool b => b,
            _ => true,
        };
    }
    
    private object Evaluate(Expr expr) {
        return expr.Accept(this);
    }

    private void Execute(Stmt stmt) {
        stmt.Accept(this);
    }

    public void ExecuteBlock(List<Stmt> statements, Environment environment) {
        Environment previous = env;
        try {
            env = environment;

            foreach (Stmt statement in statements) {
                Execute(statement);
            }
        }
        finally {
            env = previous;
        }
    }

    private string Stringify(object obj) {
        switch (obj) {
            case null: return "null";
            case double: {
                string text = obj.ToString();
                if (text.EndsWith(".0")) {
                    text = text[..^2];
                }
                return text;
            }
            default: return obj.ToString();
        }

    }
}