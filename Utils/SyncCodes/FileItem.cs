using System.Security.Cryptography;
using System.Text;
using Polly;

namespace SyncCodes;

public class FileItem
{
    public string Path { get; }

    public string? Hash { get; set; }

    public FileItem(string path)
    {
        Path = path;
    }

    public FileItem Read(string workBase, ILogger logger)
    {
        // Avoid file not exists exception, sometimes IDE creates temporary files
        try
        {
            Policy.Handle<Exception>().Retry(3, (exception, retryCount) =>
            {
                logger.LogError(
                    "Error loading file: {message}, try times: {retryCount}",
                    exception.Message,
                    retryCount
                );
            }).Execute(() =>
            {
                var content = File.ReadAllText(System.IO.Path.Combine(workBase, Path));
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
                Hash = Convert.ToBase64String(hash);
            });
        }
        catch (Exception _)
        {
        }

        return this;
    }
}

public class FileItemExistenceComparer : IEqualityComparer<FileItem>
{
    public bool Equals(FileItem? x, FileItem? y) => x?.Path.Equals(y?.Path) ?? false;

    public int GetHashCode(FileItem obj) => obj.GetHashCode();
}

public class FileItemComparer : IEqualityComparer<FileItem>
{
    public bool Equals(FileItem? x, FileItem? y) => x is not null && y is not null && x.Path.Equals(y.Path) && (x.Hash?.Equals(y.Hash) ?? false);

    public int GetHashCode(FileItem obj) => obj.GetHashCode();
}
