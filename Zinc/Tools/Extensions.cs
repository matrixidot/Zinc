namespace Zinc.Tools;

public static class Extensions {
    public static bool IsEmpty<T>(this Stack<T> stack) => stack.Count == 0;

    public static T[] ToArrayReversed<T>(this Stack<T> stack) => stack.ToArray().Reverse().ToArray();

    public static string ToString<T>(this List<T> list) => "[" + string.Join(", ", list) + "]";
    public static string ToString<T>(this T[] array) => "[" + string.Join(", ", array) + "]";
}