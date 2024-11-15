namespace Zinc.API.Interpreting;

using System.Collections;
using Builtin.Exceptions;
using Lexing;

public class Environment(Environment enclosing) {
    private readonly Environment enclosing = enclosing;
    public readonly Dictionary<string, object> values = new();

    public Environment() : this(null) {
    }

    public void Define(string name, object value) {
        values[name] = value;
    }

    public object Get(Token name) {
        if (values.TryGetValue(name.lexeme, out object value)) {
            return value;
        }
        
        if (enclosing != null) return enclosing.Get(name);
        
        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    public object GetAt(int distance, string name) {
        return Ancestor(distance).values[name];
    }

    public Environment Ancestor(int distance) {
        Environment environment = this;
        for (int i = 0; i < distance; i++) {
            environment = environment.enclosing;
        }
        return environment;
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

    public void AssignAt(int distance, Token name, object value) {
        Ancestor(distance).values[name.lexeme] = value;
    }
}