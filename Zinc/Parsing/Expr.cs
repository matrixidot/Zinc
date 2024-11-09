namespace Zinc.Expressions;

public interface Visitor<R> {
	 R VisitBinaryExpr (Binary expr);
	 R VisitGroupingExpr (Grouping expr);
	 R VisitLiteralExpr (Literal expr);
	 R VisitUnaryExpr (Unary expr);
}

public abstract class Expr {
	public abstract R Accept<R>(Visitor<R> visitor);
}

public class Binary(Expr left, Token op, Expr right) : Expr {
	public Expr Left { get; } = left;
	public Token Op { get; } = op;
	public Expr Right { get; } = right;

	public override R Accept<R>(Visitor<R> visitor) {
		return visitor.VisitBinaryExpr(this);
	}
}

public class Grouping(Expr expression) : Expr {
	public Expr Expression { get; } = expression;

	public override R Accept<R>(Visitor<R> visitor) {
		return visitor.VisitGroupingExpr(this);
	}
}

public class Literal(object value) : Expr {
	public object Value { get; } = value;

	public override R Accept<R>(Visitor<R> visitor) {
		return visitor.VisitLiteralExpr(this);
	}
}

public class Unary(Token op, Expr right) : Expr {
	public Token Op { get; } = op;
	public Expr Right { get; } = right;

	public override R Accept<R>(Visitor<R> visitor) {
		return visitor.VisitUnaryExpr(this);
	}
}

