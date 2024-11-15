﻿namespace Zinc.Lexing;

public enum TokenType {
	// Single char Tokens
	L_PAREN, R_PAREN, L_BRACE, R_BRACE, L_BRACKET, R_BRACKET,
	COMMA, DOT, PLUS, MINUS, MULT, DIV, MOD, SEMICOLON, BITWISE_AND,
	BITWISE_OR, BITWISE_XOR, BITWISE_NOT, LEFT_SHIFT, RIGHT_SHIFT,
	
	// One or two char tokens.
	NOT, NOT_EQUAL,
	ASSIGNMENT, EQUALITY,
	GREATER, GREATER_EQUAL,
	LESS, LESS_EQUAL,
	PLUS_EQUAL, MINUS_EQUAL, MULT_EQUAL, DIV_EQUAL, MOD_EQUAL,
	INCREMENT, DECREMENT, POWER, POWER_EQUAL,
	
	// Literals
	IDENTIFIER, STRING, NUMBER,
	
	// Keywords
	IF, ELIF, ELSE, TRUE, FALSE, AND, OR,
	RETURN, SUPER, THIS, FUN, NULL, PRINT, PRINTLN, VAR, CLASS,
	FOR, WHILE, BREAK, CONTINUE,
	
	
	EOF,
}