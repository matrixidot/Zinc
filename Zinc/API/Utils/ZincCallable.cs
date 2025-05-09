﻿namespace Zinc.API.Utils;

using Interpreting;

public interface ZincCallable {
    public int Arity();

    public object Call(Interpreter interpreter, List<object> arguments);
}