namespace Zinc.API.Resolving;

using Interpreting;
using Lexing;
using Parsing;
using Tools;
using Utils;

public class Resolver(Interpreter interpreter) : ExprVisitor<Void>, StmtVisitor<Void> {
    private readonly Stack<Dictionary<string, bool>> Scopes = new Stack<Dictionary<string, bool>>();
    private FunctionType currentFunc = FunctionType.NONE;
    private LoopType currentLoop = LoopType.NONE;
    private bool hadError;

    // Statements
    public Void VisitBlockStmt(Block stmt) {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public Void VisitClassStmt(Class stmt) {
        Declare(stmt.Name);
        Define(stmt.Name);
        return null;
    }

    public Void VisitVarStmt(Var stmt) {
        Declare(stmt.Name);
        if (stmt.Initializer != null) {
            Resolve(stmt.Initializer);
        }
        Define(stmt.Name);
        return null;
    }

    public Void VisitFunctionStmt(Function stmt) {
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    public Void VisitExpressionStmt(Expression stmt) {
        Resolve(stmt.Expr);
        return null;
    }

    public Void VisitIfStmt(If stmt) {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);

        foreach (Elif elif in stmt.ElifBranches) {
            Resolve(elif);
        }
        
        if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
        return null;
    }

    public Void VisitElifStmt(Elif stmt) {
        Resolve(stmt.Condition);
        Resolve(stmt.Branch);
        return null;
    }
    
    public Void VisitBreakStmt(Break stmt) {
        if (currentLoop == LoopType.NONE) {
            Zinc.Error(stmt.Keyword, "Cannot break from outside of loop");
        }
        return null;
    }

    public Void VisitContinueStmt(Continue stmt) {
        if (currentLoop == LoopType.NONE) {
            Zinc.Error(stmt.Keyword, "Cannot continue from outside of loop");
        }
        return null;
    }

    public Void VisitReturnStmt(Return stmt) {
        if (currentFunc == FunctionType.NONE) {
            Zinc.Error(stmt.Keyword, "Cannot return from top-level code.");
        }
        if (stmt.Value != null) Resolve(stmt.Value);
        return null;
    }

    public Void VisitWhileStmt(While stmt) {
        LoopType enclosingLoop = currentLoop;
        currentLoop = LoopType.LOOP;
        Resolve(stmt.Condition);
        Resolve(stmt.Body);

        if (stmt is ForLoopWithIncrement forLoop && forLoop.Increment != null) {
            Resolve(forLoop.Increment);
        }
        
        currentLoop = enclosingLoop;
        return null;
    }
    
    // Expressions
    public Void VisitVariableExpr(Variable expr) {
        if (!Scopes.IsEmpty() && Scopes.Peek().TryGetValue(expr.Name.lexeme, out bool value) && !value) {
            Zinc.Error(expr.Name, "Can't read local variable in its own initializer.");
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    public Void VisitAssignExpr(Assign expr) {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public Void VisitBinaryExpr(Binary expr) {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public Void VisitCallExpr(Call expr) {
        Resolve(expr.Callee);

        foreach (Expr argument in expr.Arguments) {
            Resolve(argument);
        }

        return null;
    }

    public Void VisitGroupingExpr(Grouping expr) {
        Resolve(expr.Expression);
        return null;
    }

    public Void VisitLiteralExpr(Literal expr) {
        return null;
    }

    public Void VisitLogicalExpr(Logical expr) {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public Void VisitUnaryExpr(Unary expr) {
        Resolve(expr.Right);
        return null;
    }

    public Void VisitIncDecExpr(IncDec expr) {
        Resolve(expr.Target);
        return null;
    }

    // Variable shit
    private void Declare(Token name) {
        if (Scopes.IsEmpty()) return;

        Dictionary<string, bool> scope = Scopes.Peek();
        if (scope.ContainsKey(name.lexeme)) {
            Zinc.Error(name, "Variable with same name already defined in this scope.");
        }
        scope[name.lexeme] = false;
    }

    private void Define(Token name) {
        if (Scopes.IsEmpty()) return;
        Scopes.Peek()[name.lexeme] = true;
    }


    // Scoping
    private void BeginScope() {
        Scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope() {
        Scopes.Pop();
    }
    
    // Resolving
    public void Resolve(List<Stmt> statements) {
        foreach (Stmt statement in statements) {
            Resolve(statement);
        }
    }
    private void Resolve(Stmt statement) {
        statement.Accept(this);
    }
    private void Resolve(Expr expr) {
        expr.Accept(this);
    }
    
    private void ResolveLocal(Expr expr, Token name) {
        for (int i = Scopes.Count - 1; i >= 0; i--) {
            if (Scopes.ToArrayReversed()[i].ContainsKey(name.lexeme)) {
                interpreter.Resolve(expr, Scopes.Count - 1 - i);
                return;
            }
        }
    }

    private void ResolveFunction(Function func, FunctionType type) {
        FunctionType enclosingFunction = currentFunc;
        currentFunc = type;
        BeginScope();
        foreach (Token param in func.Parameters) {
            Declare(param);
            Define(param);
        }
        Resolve(func.Body);
        EndScope();
        currentFunc = enclosingFunction;
    }
}

public enum FunctionType {
    NONE, FUNCTION,
}

public enum LoopType {
    NONE, LOOP,
}