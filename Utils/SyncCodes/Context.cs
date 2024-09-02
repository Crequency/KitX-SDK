namespace SyncCodes;

public class Context
{
    public string WorkBase { get; set; }

    public List<FileItem> Files { get; set; } = [];

    private FileSystemWatcher? _watcher;

    private FilesFilter _filter;

    private readonly ILogger _logger;

    public Context(string workBase, ILogger logger)
    {
        WorkBase = workBase;

        _filter = new FilesFilter(workBase, logger);

        _logger = logger;

        RefreshFiles();
    }

    public Context InitializeFileSystemWatcher(Action<FileSystemEventArgs> onChanged)
    {
        _watcher = new();
        _watcher.Path = WorkBase;
        _watcher.NotifyFilter = NotifyFilters.LastWrite
                                | NotifyFilters.FileName
                                | NotifyFilters.DirectoryName;
        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;

        var delegater = (FileSystemEventArgs e) =>
        {
            if (Path.GetRelativePath(WorkBase, e.FullPath).Equals(".sync-ignore"))
            {
                _logger.LogInformation("Sync-ignore file changed, refreshing config ...");
                _filter.LoadIgnoreConfig();
                RefreshFiles();
            }

            onChanged(e);
        };

        _watcher.Created += (_, e) => delegater(e);
        _watcher.Deleted += (_, e) => delegater(e);
        _watcher.Changed += (_, e) => delegater(e);
        _watcher.Renamed += (_, e) => delegater(e);

        return this;
    }

    public List<FileItem> GetFiles() => Files;

    public void RefreshFiles()
    {
        var folder = new DirectoryInfo(WorkBase);

        Files.Clear();
        Files.AddRange(
            folder.GetFiles()
                .Where(f => _filter.ShouldIgnore(f.FullName) == false)
                .Select(
                    f => new FileItem(
                        Path.GetRelativePath(WorkBase, f.FullName)
                    )
                )
                .Where(f => f.FileLoaded)
        );

        var foldersToSearch = new Queue<DirectoryInfo>(folder.GetDirectories());
        while (foldersToSearch.Count > 0)
        {
            var subFolder = foldersToSearch.Dequeue();
            foreach (var dir in subFolder.GetDirectories())
                foldersToSearch.Enqueue(dir);
            Files.AddRange(
                subFolder.GetFiles()
                    .Where(f => _filter.ShouldIgnore(f.FullName) == false)
                    .Select(
                        f => new FileItem(Path.GetRelativePath(WorkBase, f.FullName))
                    )
                    .Where(f => f.FileLoaded)
            );
        }
    }

    public string? GetFile(string path)
    {
        path = Path.Combine(WorkBase, path);

        return File.Exists(path) ? File.ReadAllText(path) : null;
    }
}
