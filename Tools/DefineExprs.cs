namespace Tools;

public class DefineExprs {
    public static void Run(string outputDir) {
        DefineAst(outputDir, "Expr", [
            "Binary   : Expr left, Token op, Expr right",
            "Grouping : Expr expression",
            "Literal  : object value",
            "Unary    : Token op, Expr right",
        ]);
    }
    
    private static void DefineAst(string outputDir, string baseName, List<String> types) {
        string path = $"{outputDir}/{baseName}.cs";
        File.Create(path).Close();
        File.WriteAllText(path, string.Empty);
        StreamWriter writer = new StreamWriter(path);
        writer.WriteLine("namespace Zinc.Expressions;\n");
        DefineVisitor(writer, baseName, types);
        writer.WriteLine($"public abstract class {baseName} {{");
        writer.WriteLine("\tpublic abstract R Accept<R>(Visitor<R> visitor);");
        writer.WriteLine("}\n");
		
        foreach (string type in types) {
            string className = type.Split(":")[0].Trim();
            string fields = type.Split(":")[1].Trim();
            DefineType(writer, baseName, className, fields);
        }
		
        writer.Close();
    }

    private static void DefineVisitor(StreamWriter writer, string baseName, List<String> types) {
        writer.WriteLine("public interface Visitor<R> {");

        foreach (string typeName in types.Select(type => type.Split(":")[0].Trim())) {
            writer.WriteLine($"\t R Visit{typeName}{baseName} ({typeName} {baseName.ToLower()});");
        }
		
        writer.WriteLine("}\n");
    }
	
    private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList) {
        writer.WriteLine($"public class {className}({fieldList}) : {baseName} {{");
		
        string[] fields = fieldList.Split(", ");
        foreach (string field in fields) {
            string type = field.Split(" ")[0].Trim();
            string fieldName = field.Split(" ")[1].Trim();
            fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
            writer.WriteLine($"\tpublic {type} {fieldName} {{ get; }} = {field.Split(" ")[1].Trim()};");
        }
		
        writer.WriteLine("");
        writer.WriteLine("\tpublic override R Accept<R>(Visitor<R> visitor) {");
        writer.WriteLine($"\t\treturn visitor.Visit{className}{baseName}(this);");
        writer.WriteLine("\t}");
		
        writer.WriteLine("}\n");
    }
}