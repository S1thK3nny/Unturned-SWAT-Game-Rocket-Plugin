using System.IO;
using System.Runtime.CompilerServices;

internal static class ScriptTag
{
    /// <summary>
    ///  Generates a script tag based on the calling file's name.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetScriptName([CallerFilePath] string filePath = "")
    {
        var file = Path.GetFileName(filePath);
        return $"[SWATPlugin/{file}]";
    }
}