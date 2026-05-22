#if TOOLS
using Godot;

namespace GDExtensionAPIGenerator;

[GlobalClass]
public partial class WrapperGeneratorBridge : Node
{
    public int Generate(string namespaceName, string outputPath, bool useTabs, bool exposeInternal)
    {
        var options = new WrapperGeneratorMain.WrapperGeneratorOptions(
            namespaceName,
            outputPath,
            useTabs,
            exposeInternal);

        var result = WrapperGeneratorMain.WrapperGeneratorRunner.Run(options);

        if (result.Warnings.Length != 0)
        {
            GD.PushWarning($"({result.Warnings.Length}) warning(s) during rendering the GDExtension wrapper classes:");
            foreach (var warningMessage in result.Warnings)
                GD.PushWarning(warningMessage);
        }

        return result.ExitCode;
    }
}
#endif
