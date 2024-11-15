namespace Tools;

public static class Program {
    private static string previousPath = string.Empty;
    private static string path = @"C:\Users\Romir\Desktop\Projects\C#\Zinc\Zinc\API\Parsing";

    public static void Main(string[] args) {
        DefineAsts.Run(path);
    }
}