using Zinc.Exceptions;

namespace Zinc.Parsing;

using System.Runtime.InteropServices.JavaScript;
using Lexing;
using Microsoft.VisualBasic;
using static Lexing.TokenType;

public class Parser(List<Token> tokens) {
    private List<Token> Tokens { get; } = tokens;

    private int Current { get; set; } = 0;

    private bool IsAtEnd => Peek.type == EOF;
    private Token Peek => Tokens[Current];
    private Token Previous => tokens[Current - 1];

    private int breakCount = 0;
    private int contCount = 0;

    public List<Stmt> Parse() {
        List<Stmt> statements = new();
        while (!IsAtEnd)
            statements.Add(Declaration());
        return statements;
    }
    
    private Stmt Declaration() {
        try {
            if (Match(FUN)) return Function("function");
            if (Match(VAR)) return VarDeclaration();
            
            return Statement();
        }
        catch (ParseError error) {
            Synchronize();
            return null;
        }
    }
    
    private Stmt Statement() {
        if (Match(IF)) return IfStatement();
        if (Match(PRINT)) return PrintStatement();
        if (Match(PRINTLN)) return PrintLnStatement();
        if (Match(RETURN)) return ReturnStatement();
        if (Match(WHILE)) return WhileStatement();
        if (Match(FOR)) return ForStatement();
        if (Match(BREAK)) return BreakStatement();
        if (Match(CONTINUE)) return ContinueStatement();
        if (Match(L_BRACE)) return new Block(Block());
        return ExpressionStatement();
    }

    private Break BreakStatement() {
        if (breakCount == 0) {
            throw Error(Peek, "'break' used in invalid place.");
        }
        Consume(SEMICOLON, "Expected ';' after 'break' statement.");
        return new Break();
    }

    private Continue ContinueStatement() {
        if (contCount == 0) {
            throw Error(Peek, "'continue' used in invalid place.");
        }
        Consume(SEMICOLON, "Expected ';' after 'continue' statement.");
        return new Continue();
    }
    private While WhileStatement() {
        Consume(L_PAREN, "Expected '(' after 'while'.");
        Expr condition = Expression();
        Consume(R_PAREN, "Expected ')' after condition to complete while loop.");
        breakCount++;
        contCount++;
        Stmt body = Statement();
        breakCount--;
        contCount--;
        return new While(condition, body);
    }

    private Stmt ForStatement() {
        Consume(L_PAREN, "Expected '(' after 'for'.");
    
        Stmt initializer = Match(SEMICOLON) ? null : Match(VAR) ? VarDeclaration() : ExpressionStatement();
        Expr condition = !Check(SEMICOLON) ? Expression() : new Literal(true);
        Consume(SEMICOLON, "Expected ';' after for-loop condition.");
        Expr increment = !Check(R_PAREN) ? Expression() : null;
        Consume(R_PAREN, "Expected ')' after for-loop increment.");

        breakCount++;
        contCount++;
        Stmt body = Statement();
        breakCount--;
        contCount--;
        
        if (increment != null) {
            body = new Block(new List<Stmt> { body });
        }

        Stmt loop = new ForLoopWithIncrement(condition, body, increment);

        if (initializer != null) {
            loop = new Block(new List<Stmt> { initializer, loop });
        }

        return loop;
    }
    
    private If IfStatement() {
        Consume(L_PAREN, "Expected '(' after 'if'.");
        Expr condition = Expression();
        Consume(R_PAREN, "Expected ')' after condition to complete if statement.");
        
        Stmt thenBranch = Statement();

        var elifBranches = new List<Elif>();
        while (Match(ELIF)) {
            Consume(L_PAREN, "Expected '(' after 'elif'.");
            Expr elifCondition = Expression();
            Consume(R_PAREN, "Expected ')' after condition to complete elif branch.");
            Stmt elifBranch = Statement();
            elifBranches.Add(new Elif(elifCondition, elifBranch));
        }
        
        Stmt elseBranch = null;
        if (Match(ELSE)) {
            elseBranch = Statement();
        }
        
        return new If(condition, thenBranch, elifBranches, elseBranch);
    }
    
    private List<Stmt> Block() {
        List<Stmt> statements = new();
        
        while (!Check(R_BRACE) && !IsAtEnd) statements.Add(Declaration());
        
        Consume(R_BRACE, "Expected '}' after block.");
        return statements;
    }
    
    private Var VarDeclaration() {
        Token name = Consume(IDENTIFIER, "Expected name after 'var'.");

        Expr initializer = null;
        if (Match(ASSIGNMENT)) {
            initializer = Expression();
        }
        Consume(SEMICOLON, "Expected ';' after variable declaration.");
        return new Var(name, initializer);
    }
    
    private Print PrintStatement() {
        Expr value = Expression();
        Consume(SEMICOLON, "Expected ';' after value.");
        return new Print(value);
    }

    private Println PrintLnStatement() {
        Expr value = Expression();
        Consume(SEMICOLON, "Expected ';' after value.");
        return new Println(value);
    }

    private Return ReturnStatement() {
        Token keyword = Previous;
        Expr value = null;
        if (!Check(SEMICOLON)) {
            value = Expression();
        }

        Consume(SEMICOLON, "Expected ';' after return value.");
        return new Return(keyword, value);
    }

    private Expression ExpressionStatement() {
        Expr expr = Expression();
        Consume(SEMICOLON, "Expected ';' after expression.");
        return new Expression(expr);
    }

    private Function Function(string kind) {
        Token name = Consume(IDENTIFIER, $"Expected {kind} name.");
        Consume(L_PAREN, $"Expected '(' after {kind} name.");
        List<Token> parameters = new();
        
        if (!Check(R_PAREN)) {
            do {
                if (parameters.Count >= 255) {
                    Error(Peek, "Cannot have more than 255 parameters.");
                }

                parameters.Add(Consume(IDENTIFIER, "Expected parameter name"));
            } while (Match(COMMA));
        }
        Consume(R_PAREN, $"Expected ')' after parameters to finish declaring {kind}");

        Consume(L_BRACE, $"Expected '{{' before {kind} body.");
        List<Stmt> body = Block();
        return new Function(name, parameters, body);
    }
    private Expr Expression() {
        return Assignment();
    }

    private Expr Assignment() {
        Expr expr = Or();
        
        if (Match(ASSIGNMENT, PLUS_EQUAL, MINUS_EQUAL, MULT_EQUAL, DIV_EQUAL, MOD_EQUAL, POWER_EQUAL)) {
            Token op = Previous;
            Expr value = Assignment();

            if (expr is Variable var) {
                Token name = var.Name;

                if (op.type != ASSIGNMENT) {
                    TokenType baseOpType = op.type switch {
                        PLUS_EQUAL => PLUS,
                        MINUS_EQUAL => MINUS,
                        MULT_EQUAL => MULT,
                        DIV_EQUAL => DIV,
                        MOD_EQUAL => MOD,
                        POWER_EQUAL => POWER,
                        _ => throw Error(op, "Unknown compound assignment operator."),
                    };

                    Expr compoundValue = new Binary(var, new Token(baseOpType, op.lexeme, null, op.line), value);
                    return new Assign(name, compoundValue);
                }
                
                return new Assign(name, value);
            }
            Error(op, "Invalid assignment target.");
        }
        return expr;
    }

    private Expr Or() {
        Expr expr = And();

        while (Match(OR)) {
            Token op = Previous;
            Expr right = And();
            expr = new Logical(expr, op, right);
        }
        return expr;
    }

    private Expr And() {
        Expr expr = Equality();

        while (Match(AND)) {
            Token op = Previous;
            Expr right = Equality();
            expr = new Logical(expr, op, right);
        }
        return expr;
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
        if (Match(INCREMENT, DECREMENT)) {
            Token op = Previous;
            Expr right = Unary();

            if (right is Variable var) {
                return new IncDec(op, var, isPrefix: true);
            }
            
            throw Error(op, "Invalid increment/decrement target.");
        }
        
        if (!Match(NOT, MINUS, BITWISE_NOT)) return Postfix(); {
            Token op = Previous;
            Expr right = Unary();
            return new Unary(op, right);
        }
            
    }
    
    private Expr Postfix() {
        Expr expr = Call();

        // Check for postfix increment/decrement
        if (!Match(INCREMENT, DECREMENT)) return expr;
        Token op = Previous;
        
        if (expr is Variable var) {
            return new IncDec(op, var, isPrefix: false);
        }
        
        throw Error(op, "Invalid increment/decrement target.");

    }

    private Expr Call() {
        Expr expr = Primary();

        while (true) {
            if (Match(L_PAREN)) {
                expr = FinishCall(expr);
            }
            else {
                break;
            }
        }
        return expr;
    }

    private Expr FinishCall(Expr callee) {
        List<Expr> arguments = new();
        if (!Check(R_PAREN)) {
            do {
                if (arguments.Count >= 255) {
                    Error(Peek, "Cannot have more than 255 arguments.");
                }
                arguments.Add(Expression());
            } while (Match(COMMA));
        }

        Token paren = Consume(R_PAREN, "Expected ')' after arguments");
        return new Call(callee, paren, arguments);
    }
    
    private Expr Primary() {
        if (Match(FALSE)) return new Literal(false);
        if (Match(TRUE)) return new Literal(true);
        if (Match(NULL)) return new Literal(null);
        if (Match(NUMBER, STRING)) return new Literal(Previous.literal);
        if (Match(IDENTIFIER)) return new Variable(Previous);

        if (Match(L_PAREN)) {
            Expr expr = Expression();
            Consume(R_PAREN, "Expected ')' after expression.");
            return new Grouping(expr);
        }

        throw Error(Peek, "Expected expression.");
    }
    
    // Helper Methods
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
                case CLASS or FUN or VAR or FOR or IF or WHILE or PRINT or RETURN: return;
            }

            Advance();
        }
    }
}

