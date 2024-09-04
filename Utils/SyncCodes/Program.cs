using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommandLine;
using Polly;
using Spectre.Console;
using SyncCodes;
using Context = SyncCodes.Context;
using Timer = System.Timers.Timer;

var workBase = Directory.GetCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var jsonSerializerOptions = new JsonSerializerOptions()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
};

Parser.Default.ParseArguments<CommandLineOptions>(args)
    .WithParsed(options =>
    {
        if (options.WorkBase is not null)
            workBase = options.WorkBase;

        workBase = Path.GetFullPath(workBase);

        if (options.Verbose)
            app.Logger.LogInformation("WorkBase: {workBase}", workBase);

        var context = new Context(workBase, app.Logger);

        context.InitializeFileSystemWatcher(
            (e) =>
            {
                if (context.Filter.ShouldIgnore(e.FullPath) == false)
                    app.Logger.LogInformation(
                        "[{time}] FileSystem Modified: {name}, {changeType} | {path}",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        e.Name,
                        e.ChangeType,
                        Path.GetRelativePath(workBase, e.FullPath)
                    );
                context.RefreshFiles();
            }
        );

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
    app.MapGet(
        "/",
        () => Results.Content(
            HomePageHtml(
                subTitle: "Development usage only (Server Mode)",
                content: $"""
                          <p class="mt-4 text-xl text-gray-500">🌏 Server Address: <a href="{app.Urls.First()}" target="_blank" class="font-semibold text-indigo-600 underline">{app.Urls.First()}</a></p>
                          <p class="mt-4 text-xl text-gray-500">📂 Work Base Path: <a href="file:///{workBase}" target="_blank" class="font-semibold text-indigo-600 underline">{workBase}</a></p>
                          """
            ),
            "text/html",
            Encoding.UTF8
        )
    );

    app.MapGet(
        "/catalog",
        () => context.GetFiles().Where(f => f.FileLoaded)
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
            app.Logger.LogError(
                "[{time}] Failed to fetch catalog: {statusCode}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                catalogResponse.StatusCode
            );
            return;
        }

        var catalog = JsonSerializer.Deserialize<List<FileItem>>(
            await catalogResponse.Content.ReadAsStringAsync(),
            jsonSerializerOptions
        )!;

        var localCatalog = context.GetFiles();

        var differences = catalog.Concat(localCatalog).GroupBy(
            f => f.Path.RelatedTo(workBase),
            f => f.Hash,
            (path, hashes) => new
            {
                Path = path,
                Count = hashes.Count(),
                LocalHash = localCatalog.FirstOrDefault(f => f.Path.RelatedTo(workBase).Equals(path))?.Hash,
                RemoteHash = catalog.FirstOrDefault(f => f.Path.RelatedTo(workBase).Equals(path))?.Hash,
            }
        );

        app.Logger.LogInformation(
            "Remote: {json1}\n      Local: {json2}\n      Differences: {json3}",
            JsonSerializer.Serialize(
                differences.Where(c => c.LocalHash is null && c.RemoteHash is not null),
                jsonSerializerOptions
            ),
            JsonSerializer.Serialize(
                differences.Where(c => c.RemoteHash is null && c.LocalHash is not null),
                jsonSerializerOptions
            ),
            JsonSerializer.Serialize(
                differences.Where(c => c.LocalHash is not null && c.RemoteHash is not null && !c.LocalHash.Equals(c.RemoteHash)),
                jsonSerializerOptions
            )
        );
    };
    timer.Start();

    app.MapGet(
        "/",
        () => HomePageHtml(
            subTitle: "Development usage only (Client Mode)",
            content: $"""
                      <p class="mt-4 text-xl text-gray-500">🌏 Target Server Address: <a href="{address}" target="_blank" class="font-semibold text-indigo-600 underline">{address}</a></p>
                      """
        )
    );

    app.MapGet(
        "/catalog",
        () => context.GetFiles().Where(f => f.FileLoaded)
    );

    app.Run();
}

string HomePageHtml(
    string title = "SyncCodes Util for KitX Project",
    string subTitle = "",
    string content = ""
) => $"""
      <!DOCTYPE html>
      <html>
          <head>
              <script src="https://cdn.tailwindcss.com"></script>
          </head>
          <body style="margin: 30px; font-family: Consolas, monospace;">
              <h1 class="text-4xl font-bold tracking-tight text-gray-900 sm:text-6xl">{title}</h1>
              <h3 class="mt-4 text-xl text-gray-500">{subTitle}</h3>

              <br><hr>

              {content}

              <br>

              <p>* Now is {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
          </body>
      </html>
      """;

partial class Program
{
    [GeneratedRegex(@"([0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)|(\[[0-9A-Fa-f:.]+\])|([a-zA-Z0-9.-]+)(:\d+)?")]
    private static partial Regex ServerAddressRegex();
}

public static class Extensions
{
    public static string RelatedTo(this string path, string workBase) => Path.GetRelativePath(workBase, Path.Combine(workBase, path));
}
