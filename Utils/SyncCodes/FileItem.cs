using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Polly;

namespace SyncCodes;

public class FileItem
{
    public string Path { get; }

    public string Hash { get; set; }

    [JsonIgnore] public bool FileLoaded { get; set; }

    private readonly ILogger _logger;

    public FileItem(string path, ILogger logger)
    {
        Path = path;

        _logger = logger;

        // Avoid file not exists exception, sometimes IDE creates temporary files
        Policy.Handle<Exception>().Retry(3, (exception, retryCount) =>
        {
            _logger.LogError(
                "Error loading sync ignore file: {message}, try times: {retryCount}",
                exception.Message,
                retryCount
            );

        }).Execute(() =>
        {
            var content = File.ReadAllText(Path);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            Hash = Convert.ToBase64String(hash);

            FileLoaded = true;
        });
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
