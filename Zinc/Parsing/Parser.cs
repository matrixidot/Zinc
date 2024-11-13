using Zinc.Exceptions;

namespace Zinc.Parsing;

using static TokenType;

public class Parser(List<Token> tokens) {
    private List<Token> Tokens { get; } = tokens;

    private int Current { get; set; } = 0;

    private bool IsAtEnd => Peek.type == EOF;
    private Token Peek => Tokens[Current];
    private Token Previous => tokens[Current - 1];


    public List<Stmt> Parse() {
        List<Stmt> statements = new();
        while (!IsAtEnd)
            statements.Add(Statement());
        return statements;
    }
    
    private Expr Expression() {
        return Equality();
    }

    private Stmt Statement() {
        if (Match(PRINT)) return PrintStatement();

        return ExpressionStatement();
    }

    private Stmt PrintStatement() {
        Expr value = Expression();
        Consume(SEMICOLON, "Expected ';' after value.");
        return new Print(value);
    }

    private Stmt ExpressionStatement() {
        Expr expr = Expression();
        Consume(SEMICOLON, "Expected ';' after expression.");
        return new Expression(expr);
    }
    
    private Expr Equality() {
        Expr expr = Comparison();
        while (Match(NOT_EQUAL, EQUALITY)) {
            Token op = Previous;
            Expr right = Comparison();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    private Expr Comparison() {
        Expr expr = BitwiseOr();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL)) {
            Token op = Previous;
            Expr right = BitwiseOr();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    private Expr BitwiseOr() {
        Expr expr = BitwiseXor();

        while (Match(BITWISE_OR)) {
            Token op = Previous;
            Expr right = BitwiseXor();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }
    
    private Expr BitwiseXor() {
        Expr expr = BitwiseAnd();

        while (Match(BITWISE_XOR)) {
            Token op = Previous;
            Expr right = BitwiseAnd();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }
    
    private Expr BitwiseAnd() {
        Expr expr = Shift();

        while (Match(BITWISE_AND)) {
            Token op = Previous;
            Expr right = Shift();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }
    
    private Expr Shift() {
        Expr expr = Term();

        while (Match(LEFT_SHIFT, RIGHT_SHIFT)) {
            Token op = Previous;
            Expr right = Term();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term() {
        Expr expr = Factor();

        while (Match(MINUS, PLUS)) {
            Token op = Previous;
            Expr right = Factor();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    private Expr Factor() {
        Expr expr = Exponent();
        while (Match(MULT, DIV, MOD)) {
            Token op = Previous;
            Expr right = Exponent();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    private Expr Exponent() {
        Expr expr = Unary();

        while (Match(POWER)) {
            Token op = Previous;
            Expr right = Unary();
            expr = new Binary(expr, op, right);
        }
        return expr;
    }

    private Expr Unary() {
        if (!Match(NOT, MINUS, BITWISE_NOT)) return Primary();
        Token op = Previous;
        Expr right = Unary();
        return new Unary(op, right);
    }

    private Expr Primary() {
        if (Match(FALSE)) return new Literal(false);
        if (Match(TRUE)) return new Literal(true);
        if (Match(NULL)) return new Literal(null);
        
        if (Match(NUMBER, STRING)) return new Literal(Previous.literal);

        if (Match(L_PAREN)) {
            Expr expr = Expression();
            Consume(R_PAREN, "Expected ')' after expression.");
            return new Grouping(expr);
        }

        throw Error(Peek, "Expected expression.");
    }
    
    

    private bool Match(params TokenType[] types) {
        if (!types.Any(Check)) return false;
        Advance();
        return true;
    }

    private Token Consume(TokenType type, string message) {
        if (Check(type)) return Advance();
        throw Error(Peek, message);
    }

    private bool Check(TokenType type) {
        if (IsAtEnd) return false;
        return Peek.type == type;
    }

    private Token Advance() {
        if (!IsAtEnd) Current++;
        return Previous;
    }

    private ParseError Error(Token token, string message) {
        Zinc.Error(token, message);
        return new ParseError();
    }

    private void Synchronize() {
        Advance();

        while (!IsAtEnd) {
            if (Previous.type == SEMICOLON) return;

            switch (Peek.type) {
                case CLASS or DEF or VAR or FOR or IF or WHILE or PRINT or RETURN: return;
            }

            Advance();
        }
    }
}

