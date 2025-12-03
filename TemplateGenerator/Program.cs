using System.Collections.Frozen;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace TemplateGenerator;

partial class Program
{
    const string placeholderPackageName = "!@PKG";
    const string placeholderModID = "!@MODID";
    const string placeholderModName = "!@NAME";
    const string placeholderDisplayName = "!@DISP";
    const string placeholderDescription = "!@DESC";
    static readonly UTF8Encoding UTF8NoBOM = new(false, false);

    static void Main(string[] args)
    {
        DirectoryInfo dir = new(args is [string, ..] ? args[0] :
            new(Console.ReadLine().AsSpan().Trim().Trim('"')));
        if (!dir.Exists)
        {
            Console.Error.WriteLine("""
                Usage: TemplateGenerator <Template Directory>
                Error: Directory not existed!
                """);
            return;
        }
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string dirFull = dir.FullName;
        using ZipArchive zip = ZipFile.Open($"template.{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.zip", ZipArchiveMode.Create);
        foreach (FileSystemInfo fsi in EnumerateFileSystemLeavesIncludesSelf(dir))
        {
            string newName = PathRegex()
                .Replace(Path.GetRelativePath(dirFull, fsi.FullName).Trim('\\', '/').Replace('\\', '/'), placeholderPackageName)
                .Replace("examplemodid", placeholderModID)
                .Replace("ExampleModName", placeholderModName)
                .Replace("Example Display Name", placeholderDisplayName)
                .Replace("Example Description", placeholderDescription);
            if (fsi is not FileInfo fi)
                zip.CreateEntry(newName + '/');
            else if (newName.LastIndexOf('.') is int lastDot and >= 0
                && TextFileExtensions.Contains(newName.AsSpan(lastDot + 1)))
            {
                using StreamReader reader = new(fi.FullName);
                string text = reader.ReadToEnd()
                    .Replace("net.example.examplemodid", placeholderPackageName)
                    .Replace("examplemodid", placeholderModID)
                    .Replace("ExampleModName", placeholderModName)
                    .Replace("Example Display Name", placeholderDisplayName)
                    .Replace("Example Description", placeholderDescription);
                ZipArchiveEntry entry = zip.CreateEntry(newName, CompressionLevel.SmallestSize);
                entry.LastWriteTime = now;
                using StreamWriter writer = new(entry.Open(), UTF8NoBOM);
                writer.Write(text);
            }
            else
            {
                ZipArchiveEntry entry = zip.CreateEntry(newName, CompressionLevel.SmallestSize);
                entry.LastWriteTime = now;
                using FileStream fs = fi.OpenRead();
                using Stream stream = entry.Open();
                fs.CopyTo(stream);
            }
        }
    }
    static readonly FrozenSet<string>.AlternateLookup<ReadOnlySpan<char>> TextFileExtensions = FrozenSet.Create(
        "accesswidener", "mcmeta", "txt", "md", "gradle", "java", "kt",
        "yaml", "yml", "toml", "xml", "json", "json5", "jsonc", "properties"
    ).GetAlternateLookup<ReadOnlySpan<char>>();
    static readonly EnumerationOptions Options = new()
    {
        MatchType = MatchType.Simple,
        IgnoreInaccessible = true
    };
    static IEnumerable<FileSystemInfo> EnumerateFileSystemLeavesIncludesSelf(DirectoryInfo dir)
    {
        int counter = 0;
        foreach (FileSystemInfo it in dir.EnumerateFileSystemInfos("*", Options))
        {
            counter++;
            if (it is DirectoryInfo di)
                foreach (FileSystemInfo iit in EnumerateFileSystemLeavesIncludesSelf(di))
                    yield return iit;
            else
                yield return it;
        }
        if (counter == 0)
            yield return dir;
    }
    [GeneratedRegex("net[./]example[./]examplemodid")]
    private static partial Regex PathRegex();
}