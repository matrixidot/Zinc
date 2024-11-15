namespace Zinc.Parsing;

using Lexing;

public abstract class Expr {

public interface ExprVisitor<R> {
	 R VisitAssignExpr (Assign expr);
	 R VisitBinaryExpr (Binary expr);
	 R VisitCallExpr (Call expr);
	 R VisitGroupingExpr (Grouping expr);
	 R VisitLiteralExpr (Literal expr);
	 R VisitLogicalExpr (Logical expr);
	 R VisitUnaryExpr (Unary expr);
	 R VisitIncDecExpr (IncDec expr);
	 R VisitVariableExpr (Variable expr);
}

	public abstract R Accept<R>(ExprVisitor<R> visitor);
}

public class Assign(Token name, Expr value) : Expr {
	public Token Name { get; } = name;
	public Expr Value { get; } = value;

	public override R Accept<R>(ExprVisitor<R> visitor) {
		return visitor.VisitAssignExpr(this);
	}
}

public class Binary(Expr left, Token op, Expr right) : Expr {
	public Expr Left { get; } = left;
	public Token Op { get; } = op;
	public Expr Right { get; } = right;

	public override R Accept<R>(ExprVisitor<R> visitor) {
		return visitor.VisitBinaryExpr(this);
	}
}

public class Call(Expr callee, Token paren, List<Expr> arguments) : Expr {
	public Expr Callee { get; } = callee;
	public Token Paren { get; } = paren;
	public List<Expr> Arguments { get; } = arguments;

	public override R Accept<R>(ExprVisitor<R> visitor) {
		return visitor.VisitCallExpr(this);
	}
}

public class Grouping(Expr expression) : Expr {
	public Expr Expression { get; } = expression;

	public override R Accept<R>(ExprVisitor<R> visitor) {
		return visitor.VisitGroupingExpr(this);
	}
}

public class Literal(object value) : Expr {
	public object Value { get; } = value;

	public override R Accept<R>(ExprVisitor<R> visitor) {
		return visitor.VisitLiteralExpr(this);
	}
}

public class Logical(Expr left, Token op, Expr right) : Expr {
	public Expr Left { get; } = left;
	public Token Op { get; } = op;
	public Expr Right { get; } = right;

	public override R Accept<R>(ExprVisitor<R> visitor) {
		return visitor.VisitLogicalExpr(this);
	}
}

public class Unary(Token op, Expr right) : Expr {
	public Token Op { get; } = op;
	public Expr Right { get; } = right;

	public override R Accept<R>(ExprVisitor<R> visitor) {
		return visitor.VisitUnaryExpr(this);
	}
}

public class IncDec(Token op, Variable target, bool isPrefix) : Expr {
	public Token Op { get; } = op;
	public Variable Target { get; } = target;
	public bool IsPrefix { get; } = isPrefix;

	public override R Accept<R>(ExprVisitor<R> visitor) {
		return visitor.VisitIncDecExpr(this);
	}
}

public class Variable(Token name) : Expr {
	public Token Name { get; } = name;

	public override R Accept<R>(ExprVisitor<R> visitor) {
		return visitor.VisitVariableExpr(this);
	}
}

