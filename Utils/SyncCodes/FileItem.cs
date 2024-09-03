using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace SyncCodes;

public class FileItem
{
    public string Path { get; }

    public string Hash { get; }

    [JsonIgnore] public bool FileLoaded { get; set; }

    public FileItem(string path)
    {
        Path = path;

        // Avoid file not exists exception, sometimes IDE creates temporary files
        try
        {
            var content = File.ReadAllText(Path);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            Hash = Convert.ToBase64String(hash);

            FileLoaded = true;
        }
        catch (Exception e)
        {
            Hash = string.Empty;

            FileLoaded = false;
        }
    }
}

public class FileItemExistenceComparer : IEqualityComparer<FileItem>
{
    public bool Equals(FileItem? x, FileItem? y) => x?.Path?.Equals(y?.Path) ?? false;

    public int GetHashCode(FileItem obj) => obj.GetHashCode();
}

public class FileItemComparer : IEqualityComparer<FileItem>
{
    public bool Equals(FileItem? x, FileItem? y) => x is not null && y is not null && x.FileLoaded && y.FileLoaded && x.Path.Equals(y.Path) && x.Hash.Equals(y.Hash);

    public int GetHashCode(FileItem obj) => obj.GetHashCode();
}
