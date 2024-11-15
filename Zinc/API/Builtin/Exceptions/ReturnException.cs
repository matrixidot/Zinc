namespace Zinc.API.Builtin.Exceptions;

public class ReturnException(object value) : SystemException {
    public object Value { get; } = value;
}