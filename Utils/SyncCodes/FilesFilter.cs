using Polly;

namespace SyncCodes;

public class FilesFilter
{
    public readonly List<string> IgnoredExtensions = [];

    public readonly List<string> IgnoredPaths = [];

    public string WorkBase { get; init; }

    private bool _configFileExists = true;

    private readonly ILogger _logger;

    public FilesFilter(string workBase, ILogger logger)
    {
        _logger = logger;

        WorkBase = workBase;

        LoadIgnoreConfig();
    }

    public FilesFilter LoadIgnoreConfig()
    {
        IgnoredExtensions.Clear();
        IgnoredPaths.Clear();

        var path = Path.Combine(WorkBase, ".sync-ignore");

        if (!File.Exists(path))
        {
            _configFileExists = false;
            return this;
        }

        Policy
            .Handle<Exception>()
            .Retry(3, (exception, retryCount, context) =>
            {
                _logger.LogError(
                    "Error loading sync ignore file: {message}, try times: {retryCount}",
                    exception.Message,
                    retryCount
                );
            })
            .Execute(() =>
            {
                var lines = File.ReadAllLines(path);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.Trim().StartsWith('#'))
                        continue;

                    if (line.StartsWith('.') && !line.EndsWith('/'))
                        IgnoredExtensions.Add(line.Trim().ToLower());
                    else
                    {
                        var fullPath = Path.Combine(WorkBase, line.Trim());
                        IgnoredPaths.Add(Path.GetRelativePath(WorkBase, fullPath));
                    }
                }
            });

        return this;
    }

    public bool ShouldIgnore(string path)
    {
        if (_configFileExists == false) return false;

        var ext = Path.GetExtension(path).ToLower();

        if (IgnoredExtensions.Contains(ext))
            return true;

        path = Path.GetRelativePath(WorkBase, path);

        if (IgnoredPaths.Any(p => path.StartsWith(p)))
            return true;

        return false;
    }
}
