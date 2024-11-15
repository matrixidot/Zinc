using a;

namespace Tools;

public class DefineAsts {
    public static void Run(string outputDir) {
        DefineAst(outputDir, "Expr", [
            "Assign         : Token name, Expr value",
            "Binary         : Expr left, Token op, Expr right",
            "Call           : Expr callee, Token paren, List<Expr> arguments",
            "Grouping       : Expr expression",
            "Literal        : object value",
            "Logical        : Expr left, Token op, Expr right",
            "Unary          : Token op, Expr right",
            "IncDec         : Token op, Variable target, bool isPrefix",
            "Variable       : Token name",
        ]);        
        DefineAst(outputDir, "Stmt", [
            "Block      : List<Stmt> statements",
            "Expression : Expr expr",
            "Function   : Token name, List<Token> parameters, List<Stmt> body",
            "If         : Expr condition, Stmt thenBranch, List<Elif> elifBranches, Stmt elseBranch",
            "Elif       : Expr condition, Stmt branch",
            "Print      : Expr expr",
            "Println    : Expr expr",
            "Return     : Token keyword, Expr value",
            "Var        : Token name, Expr initializer",
            "While      : Expr condition, Stmt body",
            "Break      : ",
            "Continue   : ",
        ]);
    }
    
    private static void DefineAst(string outputDir, string baseName, List<String> types) {
        string path = $"{outputDir}/{baseName}.cs";
        File.Create(path).Close();
        File.WriteAllText(path, string.Empty);
        StreamWriter writer = new StreamWriter(path);
        writer.WriteLine("namespace Zinc.Parsing;\n");
        writer.WriteLine($"public abstract class {baseName} {{\n");
        DefineVisitor(writer, baseName, types);
        writer.WriteLine($"\tpublic abstract R Accept<R>({baseName.CFL()}Visitor<R> visitor);");
        writer.WriteLine("}\n");
		
        foreach (string type in types) {
            string className = type.Split(":")[0].Trim();
            string fields = type.Split(":")[1].Trim();
            DefineType(writer, baseName, className, fields);
        }
		
        writer.Close();
    }

    private static void DefineVisitor(StreamWriter writer, string baseName, List<String> types) {
        writer.WriteLine($"public interface {baseName.CFL()}Visitor<R> {{");

        foreach (string typeName in types.Select(type => type.Split(":")[0].Trim())) {
            writer.WriteLine($"\t R Visit{typeName}{baseName} ({typeName} {baseName.ToLower()});");
        }
		
        writer.WriteLine("}\n");
    }
	
    private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList) {
        if (fieldList == string.Empty) {
            writer.WriteLine($"public class {className} : {baseName} {{");

        }
        else {
            writer.WriteLine($"public class {className}({fieldList}) : {baseName} {{");
            string[] fields = fieldList.Split(", ");
            foreach (string field in fields) {
                string type = field.Split(" ")[0].Trim();
                string fieldName = field.Split(" ")[1].Trim();
                fieldName = fieldName.CFL();
                writer.WriteLine($"\tpublic {type} {fieldName} {{ get; }} = {field.Split(" ")[1].Trim()};");
            }
        }
        
        
		
        writer.WriteLine("");
        writer.WriteLine($"\tpublic override R Accept<R>({baseName.CFL()}Visitor<R> visitor) {{");
        writer.WriteLine($"\t\treturn visitor.Visit{className}{baseName}(this);");
        writer.WriteLine("\t}");
		
        writer.WriteLine("}\n");
    }
}