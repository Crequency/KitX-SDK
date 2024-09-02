using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommandLine;
using Spectre.Console;
using SyncCodes;
using Timer = System.Timers.Timer;

var workBase = Directory.GetCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

Parser.Default.ParseArguments<CommandLineOptions>(args)
    .WithParsed(options =>
    {
        if (options.WorkBase is not null)
            workBase = options.WorkBase;

        workBase = Path.GetFullPath(workBase);

        if (options.Verbose)
            app.Logger.LogInformation("WorkBase: {workBase}", workBase);

        var context = new Context(workBase, app.Logger);

        switch (options.Job)
        {
            case Jobs.FetchCodes:
                RunClient(context);
                break;
            case Jobs.ProvideCodes:
                RunServer(context);
                break;
            default:
                app.Logger.LogWarning("Meet unknown job: {job}", options.Job.ToString());
                break;
        }
    })
    ;

return;

void RunServer(Context context)
{
    context.InitializeFileSystemWatcher(
        (e) =>
        {
            app.Logger.LogInformation("FileSystem Modified: {message}", e.ToString());
            context.RefreshFiles();
        }
    );

    app.MapGet(
        "/",
        () => $"""
               SyncCodes Util for KitX Project
               Development usage only (Server Mode)

               * Server Address: {app.Urls.First()}

               Now is `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`
               """
    );

    app.MapGet(
        "/catalog",
        context.GetFiles
    );

    app.MapGet(
        "/file/{path}",
        (string path) => context.GetFile(
            Encoding.UTF8.GetString(
                Convert.FromBase64String(path)
            )
        )
    );

    app.Run();
}

void RunClient(Context context)
{
    context.InitializeFileSystemWatcher((_) => context.RefreshFiles());

    var address = AnsiConsole.Prompt(new TextPrompt<string>("Please input the server address: ")
    {
        Validator = (s) => ServerAddressRegex().IsMatch(s)
            ? ValidationResult.Success()
            : ValidationResult.Error($"Invalid server address: `{s}`")
    });

    const string protocol = "http";
    var catalogApi = $"{protocol}://{address}/catalog";
    var fileApi = $"{protocol}://{address}/file/";

    var client = new HttpClient();

    var timer = new Timer(1000)
    {
        AutoReset = true,
    };
    timer.Elapsed += async (_, _) =>
    {
        var catalogResponse = await client.GetAsync(catalogApi);
        if (!catalogResponse.IsSuccessStatusCode)
        {
            return;
        }

        var catalog = JsonSerializer.Deserialize<List<FileItem>>(
            await catalogResponse.Content.ReadAsStringAsync()
        )!;

        var localCatalog = context.GetFiles();

        var needToFetch = catalog.Except(localCatalog);
        var needToDelete = localCatalog.Except(catalog);
        var needToUpdate = localCatalog.GroupJoin(
                    catalog,
                    x => x,
                    y => y,
                    (x, y) => new { Original = x, New = y }
                )
                .SelectMany(
                    c => c.New.DefaultIfEmpty(),
                    (x, y) => new { x.Original, New = y }
                )
                .Where(c => c.Original.Hash.Equals(c.New!.Hash) == false)
                .Select(c => new { Original = c.Original, Difference = c.New })
            ;

        app.Logger.LogDebug("Need to fetch: {json}", JsonSerializer.Serialize(needToFetch));
        app.Logger.LogDebug("Need to delete:: {json}", JsonSerializer.Serialize(needToDelete));
        app.Logger.LogDebug("Need to update: {json}", JsonSerializer.Serialize(needToUpdate));
    };
    timer.Start();

    app.MapGet(
        "/",
        () => $"""
               SyncCodes Util for KitX Project
               Development usage only (Client Mode)

               * Target Server Address: {address}

               Now is `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`
               """
    );

    app.Run();
}

partial class Program
{
    [GeneratedRegex(@"([0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)|(\[[0-9A-Fa-f:.]+\])|([a-zA-Z0-9.-]+)(:\d+)?")]
    private static partial Regex ServerAddressRegex();
}
