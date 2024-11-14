﻿namespace Zinc.Interpreting;

using Exceptions;

public class Environment {
    private readonly Environment enclosing;
    private readonly Dictionary<string, object> values = new();

    public Environment() {
        enclosing = null;
    }

    public Environment(Environment enclosing) {
        this.enclosing = enclosing;
    }
    
    public void Declare(string name, object value) {
        values[name] = value;
    }

    public object Get(Token name) {
        if (values.TryGetValue(name.lexeme, out object? value)) {
            return value;
        }
        
        if (enclosing != null) return enclosing.Get(name);
        
        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    public void Assign(Token name, object value) {
        if (values.ContainsKey(name.lexeme)) {
            values[name.lexeme] = value;
            return;
        }

        if (enclosing != null) {
            enclosing.Assign(name, value);
            return;
        }
        
        throw new RuntimeError(name, $"Cannot assign to undefined variable '{name.lexeme}'.");
    }
}