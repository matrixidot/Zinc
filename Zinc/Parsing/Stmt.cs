namespace Zinc.Parsing;

public abstract class Stmt {

public interface StmtVisitor<R> {
	 R VisitBlockStmt (Block stmt);
	 R VisitExpressionStmt (Expression stmt);
	 R VisitPrintStmt (Print stmt);
	 R VisitVarStmt (Var stmt);
}

	public abstract R Accept<R>(StmtVisitor<R> visitor);
}

public class Block(List<Stmt> statements) : Stmt {
	public List<Stmt> Statements { get; } = statements;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitBlockStmt(this);
	}
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

public class Var(Token name, Expr initializer) : Stmt {
	public Token Name { get; } = name;
	public Expr Initializer { get; } = initializer;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitVarStmt(this);
	}
}

