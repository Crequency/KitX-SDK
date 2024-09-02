using System.Security.Cryptography;
using System.Text;

namespace SyncCodes;

public class FileItem
{
    public string Path { get; set; }

    public string Hash { get; set; }

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
}
