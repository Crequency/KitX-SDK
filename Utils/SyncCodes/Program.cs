using System.Text;
using CommandLine;
using SyncCodes;

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

        if (options.Job == Jobs.ProvideCodes)
            context.InitializeFileSystemWatcher(
                (e) =>
                {
                    app.Logger.LogInformation("FileSystem Modified: {message}", e.ToString());
                    context.RefreshFiles();
                }
            );

        switch (options.Job)
        {
            case Jobs.FetchCodes:
                break;
            case Jobs.ProvideCodes:
                app.MapGet(
                    "/",
                    () => $"""
                           SyncCodes Util for KitX Project
                           Development usage only

                           Now is `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`
                           """
                );

                app.MapGet(
                    "/catalog",
                    () => context.GetFiles()
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
                break;
            default:
                app.Logger.LogWarning("Meet unknown job: {job}", options.Job.ToString());
                break;
        }
    })
    ;
