namespace Zinc.Parsing;

public abstract class Stmt {

public interface StmtVisitor<R> {
	 R VisitExpressionStmt (Expression stmt);
	 R VisitPrintStmt (Print stmt);
}

	public abstract R Accept<R>(StmtVisitor<R> visitor);
}

public class Expression(Expr expr) : Stmt {
	public Expr Expr { get; } = expr;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitExpressionStmt(this);
	}
}

public class Print(Expr expr) : Stmt {
	public Expr Expr { get; } = expr;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitPrintStmt(this);
	}
}

