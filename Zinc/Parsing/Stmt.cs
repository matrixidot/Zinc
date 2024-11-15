namespace Zinc.Parsing;

using Lexing;

public abstract class Stmt {

public interface StmtVisitor<R> {
	 R VisitBlockStmt (Block stmt);
	 R VisitExpressionStmt (Expression stmt);
	 R VisitFunctionStmt (Function stmt);
	 R VisitIfStmt (If stmt);
	 R VisitElifStmt (Elif stmt);
	 R VisitPrintStmt (Print stmt);
	 R VisitPrintlnStmt (Println stmt);
	 R VisitReturnStmt (Return stmt);
	 R VisitVarStmt (Var stmt);
	 R VisitWhileStmt (While stmt);
	 R VisitBreakStmt (Break stmt);
	 R VisitContinueStmt (Continue stmt);
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

public class Function(Token name, List<Token> parameters, List<Stmt> body) : Stmt {
	public Token Name { get; } = name;
	public List<Token> Parameters { get; } = parameters;
	public List<Stmt> Body { get; } = body;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitFunctionStmt(this);
	}
}

public class If(Expr condition, Stmt thenBranch, List<Elif> elifBranches, Stmt elseBranch) : Stmt {
	public Expr Condition { get; } = condition;
	public Stmt ThenBranch { get; } = thenBranch;
	public List<Elif> ElifBranches { get; } = elifBranches;
	public Stmt ElseBranch { get; } = elseBranch;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitIfStmt(this);
	}
}

public class Elif(Expr condition, Stmt branch) : Stmt {
	public Expr Condition { get; } = condition;
	public Stmt Branch { get; } = branch;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitElifStmt(this);
	}
}

public class Print(Expr expr) : Stmt {
	public Expr Expr { get; } = expr;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitPrintStmt(this);
	}
}

public class Println(Expr expr) : Stmt {
	public Expr Expr { get; } = expr;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitPrintlnStmt(this);
	}
}

public class Return(Token keyword, Expr value) : Stmt {
	public Token Keyword { get; } = keyword;
	public Expr Value { get; } = value;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitReturnStmt(this);
	}
}

public class Var(Token name, Expr initializer) : Stmt {
	public Token Name { get; } = name;
	public Expr Initializer { get; } = initializer;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitVarStmt(this);
	}
}

public class While(Expr condition, Stmt body) : Stmt {
	public Expr Condition { get; } = condition;
	public Stmt Body { get; } = body;

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitWhileStmt(this);
	}
}

public class Break : Stmt {

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitBreakStmt(this);
	}
}

public class Continue : Stmt {

	public override R Accept<R>(StmtVisitor<R> visitor) {
		return visitor.VisitContinueStmt(this);
	}
}

