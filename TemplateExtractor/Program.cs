using System.Collections.Frozen;
using System.IO.Compression;
using System.Text;

namespace TemplateExtractor;

class Program
{
    const string placeholderPackageName = "!@PKG";
    const string placeholderModID = "!@MODID";
    const string placeholderModName = "!@NAME";
    const string placeholderDisplayName = "!@DISP";
    const string placeholderDescription = "!@DESC";
    static readonly UTF8Encoding UTF8NoBOM = new(false, false);

    static void Main(string[] args)
    {
        string path = args is [string, ..] ? args[0] :
            Console.ReadLine().AsSpan().Trim().Trim('"') is { IsEmpty: false } name ? new(name) :
            "template.zip";
        if (!File.Exists(path))
        {
            Console.Error.WriteLine("""
                Usage: TemplateExtractor <Template ZIP>
                Error: File not existed!
                """);
            return;
        }
        using ZipArchive zip = ZipFile.OpenRead(path);
        Console.WriteLine("Package name:");
        string packageName = Console.ReadLine()?.Trim() ?? "";
        if (!ValidatePackageName(packageName))
        {
            Console.Error.WriteLine("Error: Invalid package name!");
            return;
        }
        string packageNameSlash = packageName.Replace('.', '/');
        Console.WriteLine("Mod ID:");
        string modID = Console.ReadLine()?.Trim() ?? "";
        if (!ValidateModID(modID))
        {
            Console.Error.WriteLine("Error: Invalid mod ID!");
            return;
        }
        Console.WriteLine("Mod name:");
        string modName = Console.ReadLine()?.Trim() ?? "";
        if (!ValidateModName(modName))
        {
            Console.Error.WriteLine("Error: Invalid mod name!");
            return;
        }
        Console.WriteLine("Display name:");
        string displayName = Console.ReadLine()?.Trim() ?? "";
        Console.WriteLine("Description:");
        string description = Console.ReadLine()?.Trim() ?? "";
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            string newName = modName + '/' + entry.FullName
                .Replace(placeholderPackageName, packageNameSlash)
                .Replace(placeholderModID, modID)
                .Replace(placeholderModName, modName)
                .Replace(placeholderDisplayName, displayName)
                .Replace(placeholderDescription, description);
            if (newName.EndsWith('/'))
            {
                Directory.CreateDirectory(newName);
                continue;
            }
            if (newName.LastIndexOf('/') is int lastSlash and >= 0)
                Directory.CreateDirectory(newName[..lastSlash]);
            if (newName.LastIndexOf('.') is int lastDot and >= 0
                && TextFileExtensions.Contains(newName.AsSpan(lastDot + 1)))
            {
                using StreamReader reader = new(entry.Open());
                string text = reader.ReadToEnd()
                    .Replace(placeholderPackageName, packageName)
                    .Replace(placeholderModID, modID)
                    .Replace(placeholderModName, modName)
                    .Replace(placeholderDisplayName, displayName)
                    .Replace(placeholderDescription, description);
                File.WriteAllText(newName, text, UTF8NoBOM);
            }
            else
            {
                using Stream stream = entry.Open();
                using FileStream fs = File.Open(newName, FileMode.Create, FileAccess.Write, FileShare.Read);
                stream.CopyTo(fs);
            }
        }
    }
    static readonly FrozenSet<string>.AlternateLookup<ReadOnlySpan<char>> TextFileExtensions = FrozenSet.Create(
        "accesswidener", "mcmeta", "txt", "md", "gradle", "java", "kt",
        "yaml", "yml", "toml", "xml", "json", "json5", "jsonc", "properties"
    ).GetAlternateLookup<ReadOnlySpan<char>>();
    static readonly FrozenSet<string>.AlternateLookup<ReadOnlySpan<char>> JavaKeywords = FrozenSet.Create(
        "abstract", "assert", "boolean", "break", "byte", "case", "catch", "char",
        "class", "const", "continue", "default", "do", "double", "else", "enum",
        "extends", "final", "finally", "float", "for", "goto", "if", "implements",
        "import", "instanceof", "int", "interface", "long", "native", "new",
        "package", "private", "protected", "public", "return", "short", "static",
        "strictfp", "super", "switch", "synchronized", "this", "throw", "throws",
        "transient", "try", "void", "volatile", "while"
    ).GetAlternateLookup<ReadOnlySpan<char>>();
    static bool ValidatePackageName(ReadOnlySpan<char> packageName)
    {
        foreach (Range range in packageName.Split('.'))
        {
            ReadOnlySpan<char> slice = packageName[range];
            if (slice.IsEmpty
                || JavaKeywords.Contains(slice)
                || slice[0] is not ((>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_'))
                return false;
            for (int i = 1; i < slice.Length; i++)
            {
                if (slice[i] is not ((>= '0' and <= '9') or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_'))
                    return false;
            }
        }
        return true;
    }
    static bool ValidateModID(ReadOnlySpan<char> modID)
    {
        if (modID.IsEmpty
            || modID[0] is not (>= 'a' and <= 'z'))
            return false;
        for (int i = 1; i < modID.Length; i++)
        {
            if (modID[i] is not ((>= '0' and <= '9') or (>= 'a' and <= 'z') or '_' or '-'))
                return false;
        }
        return true;
    }
    static bool ValidateModName(ReadOnlySpan<char> modName)
    {
        if (modName.IsEmpty
            || modName[0] is not ((>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_' or '$'))
            return false;
        for (int i = 1; i < modName.Length; i++)
        {
            if (modName[i] is not ((>= '0' and <= '9') or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_' or '$'))
                return false;
        }
        return true;
    }
}