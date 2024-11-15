using Zinc.Exceptions;
using Zinc.Parsing;
using Void = Zinc.Tools.Void;

namespace Zinc.Interpreting;

using System.Xml.Xsl;

public class Interpreter : Expr.ExprVisitor<object>, Stmt.StmtVisitor<Void> {
    private Environment env = new();
    private bool suppressOutput = false;
    public void Interpret(List<Stmt> statements) {
        try {
            if (statements.Count == 1 && statements[0] is Expression expr) {
                object value = Evaluate(expr.Expr);
                Console.WriteLine(Stringify(value));
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

    public object VisitVariableExpr(Variable expr) {
        return env.Get(expr.Name);
    }

    public object VisitAssignExpr(Assign expr) {
        object value = Evaluate(expr.Value);
        env.Assign(expr.Name, value);
        return value;
    }
    
    public object VisitIncDecExpr(IncDec expr) {
        // Retrieve the variable from the environment
        object value = env.Get(expr.Target.Name);
    
        // Ensure the variable is a number
        if (value is not double oldValue) {
            throw new RuntimeError(expr.Op, $"Operand for {expr.Op.lexeme} must be a number.");
        }

        // Perform increment or decrement
        double newValue = expr.Op.type == TokenType.INCREMENT ? oldValue + 1 : oldValue - 1;

        // Assign the new value back to the variable
        env.Assign(expr.Target.Name, newValue);

        // Return the correct value based on prefix or postfix
        return expr.IsPrefix ? newValue : oldValue;
    }

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

    public Void VisitExpressionStmt(Expression stmt) {
        Evaluate(stmt.Expr);
        return null;
    }
    
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

    public Void VisitIfStmt(If stmt) {
        if (IsTruthy(Evaluate(stmt.Condition))) {
            Execute(stmt.ThenBranch);
            
        } else {
            bool elifBranchExecuted = false;
            foreach (var elif in stmt.ElifBranches) {
                if (IsTruthy(Evaluate(elif.Condition))) {
                    Execute(elif.Branch);
                    elifBranchExecuted = true;
                    break;
                }
            }

            if (!elifBranchExecuted && stmt.ElseBranch != null) {
                Execute(stmt.ElseBranch);
            }
        }
        return null;
    }

    public Void VisitElifStmt(Elif stmt) {
        throw new RuntimeError(new Token(TokenType.ELIF, "elif", null, 0), "Unexpected elif Statement, Are you missing an if?");
    }

    public Void VisitBlockStmt(Block stmt) {
        ExecuteBlock(stmt.Statements, new Environment(env));
        return null;
    }

    public Void VisitWhileStmt(While stmt) {
        try {
            while (IsTruthy(Evaluate(stmt.Condition))) {
                try {
                    Execute(stmt.Body);
                }
                catch (ContinueError) {
                    // Skip to next iteration by breaking out of body and moving to increment
                }

                // If this is a `for` loop with an increment, evaluate the increment
                if (stmt is ForLoopWithIncrement forLoopStmt) {
                    Evaluate(forLoopStmt.Increment);
                }
            }
        } catch (BreakError) {
            // Exit the loop on `break`
        }
        return null;
    }


    public Void VisitBreakStmt(Break stmt) {
        throw new BreakError();
    }

    public Void VisitContinueStmt(Continue stmt) {
        throw new ContinueError();
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

    private void ExecuteBlock(List<Stmt> statements, Environment environment) {
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