namespace Zinc.Tools;

using System.Text;
using Expressions;

public class AstPrinter : Visitor<string> {
    public string print(Expr expr) {
        return expr.Accept(this);
    }

    public string VisitBinaryExpr(Binary expr) => Parenthesize(expr.Op.lexeme, expr.Left, expr.Right);

    public string VisitGroupingExpr(Grouping expr) => Parenthesize("group", expr.Expression);

    public string VisitLiteralExpr(Literal expr) => expr.Value == null ? "null" : expr.Value.ToString();

    public string VisitUnaryExpr(Unary expr) => Parenthesize(expr.Op.lexeme, expr.Right);


    private string Parenthesize(string name, params Expr[] exprs) {
        StringBuilder sb = new();
        sb.Append("(").Append(name);
        foreach (Expr expr in exprs) {
            sb.Append(" ").Append(expr.Accept(this));
        }
        sb.Append(")");

        return sb.ToString();
    }
}