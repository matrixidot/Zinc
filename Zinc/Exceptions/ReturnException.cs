namespace Zinc.Exceptions;

public class ReturnException(object value) : SystemException {
    public object Value { get; } = value;
}