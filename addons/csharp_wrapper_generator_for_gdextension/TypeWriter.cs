using System.Collections.Concurrent;
using System.Text;

namespace GDExtensionAPIGenerator;

public partial class WrapperGeneratorMain
{
    public record WrapperFile(string FileName, string SourceCode);
    
    private static class TypeWriter
    {
        public static void WriteType(GodotClassType type, ConcurrentBag<WrapperFile> files, ConcurrentBag<string> warnings)
        {
            var fileBuilder = new StringBuilder();
            type.RenderClass(fileBuilder, warnings);

            var code = fileBuilder.ToString();
            files.Add(new($"{type.CSharpTypeName}.wrapper.cs", code));
        }
    }
}