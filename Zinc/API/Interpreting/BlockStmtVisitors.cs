namespace Zinc.API.Interpreting;

using Builtin.Exceptions;
using Lexing;
using Parsing;
using Utils;
using Void = Utils.Void;
public partial class Interpreter {
    public Void VisitIfStmt(If stmt) {
        if (IsTruthy(Evaluate(stmt.Condition))) {
            Execute(stmt.ThenBranch);
            
        } else {
            bool elifBranchExecuted = false;
            foreach (Elif elif in stmt.ElifBranches.Where(elif => IsTruthy(Evaluate(elif.Condition)))) {
                Execute(elif.Branch);
                elifBranchExecuted = true;
                break;
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

    public Void VisitClassStmt(Class stmt) {
        env.Define(stmt.Name.lexeme, null);
        ZincClass clazz = new(stmt.Name.lexeme);
        env.Assign(stmt.Name, clazz);
        return null;
    }

    public Void VisitWhileStmt(While stmt) {
        try {
            while (IsTruthy(Evaluate(stmt.Condition))) {
                try {
                    Execute(stmt.Body);
                }
                catch (ContinueError) { }
                
                if (stmt is ForLoopWithIncrement forLoopStmt) {
                    Evaluate(forLoopStmt.Increment);
                }
            }
        } catch (BreakError) { }
        return null;
    }

    public Void VisitFunctionStmt(Function stmt) {
        ZincFunction function = new ZincFunction(stmt, env);
        env.Define(stmt.Name.lexeme, function);
        return null;
    }
    
    public Void VisitExpressionStmt(Expression stmt) {
        Evaluate(stmt.Expr);
        return null;
    }
}