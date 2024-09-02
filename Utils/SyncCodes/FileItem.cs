using System.Security.Cryptography;
using System.Text;

namespace SyncCodes;

public class FileItem
{
    public string Path { get; }

    public string Hash { get; }

    public bool FileLoaded { get; set; }

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

    public override bool Equals(object? obj) => obj is FileItem item && FileLoaded && item.FileLoaded && Hash.Equals(item.Hash);

    public override int GetHashCode() => Convert.FromBase64String(Hash).GetHashCode();
}
