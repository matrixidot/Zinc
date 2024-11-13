namespace Zinc.Tools;

using Parsing;

public class AstPrinter : Expr.ExprVisitor<string> {
    public string print(Expr expr) {
        return expr.Accept(this);
    }

    public string VisitBinaryExpr(Binary expr) => $"({expr.Left.Accept(this)} {expr.Op.lexeme} {expr.Right.Accept(this)})";

    public string VisitGroupingExpr(Grouping expr) => $"{expr.Expression.Accept(this)}";

    public string VisitLiteralExpr(Literal expr) => expr.Value == null ? "null" : expr.Value.ToString();

    public string VisitUnaryExpr(Unary expr) => $"{expr.Op.lexeme}{expr.Right.Accept(this)}";
    
}