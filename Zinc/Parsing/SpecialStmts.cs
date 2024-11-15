namespace Zinc.Parsing;

public class ForLoopWithIncrement(Expr condition, Stmt body, Expr increment) : While(condition, body) {
    public Expr Increment { get; } = increment;
}