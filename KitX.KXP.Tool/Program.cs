using CommandLine;
using Common.BasicHelper.Utils.Extensions;
using KitX.KXP.Tool;
using System.Text;
using Encoder = KitX.Formats.KXP.Encoder;

Parser.Default.ParseArguments<Options, PackOptions>(args)
    .WithParsed<PackOptions>(options =>
    {
        options.SourcePath ??= "./".GetFullPath();
        options.SourcePath = options.SourcePath.GetFullPath();
        options.OutputPath = options.OutputPath.GetFullPath();
        options.OutputFileName ??= new DirectoryInfo(options.SourcePath).Name ?? "Plugin";
        options.LoaderStructPath ??= $"{options.SourcePath}/LoaderStruct.json".GetFullPath();
        options.LoaderStructPath = options.LoaderStructPath.GetFullPath();
        options.PluginStructPath ??= $"{options.SourcePath}/PluginStruct.json".GetFullPath();
        options.PluginStructPath = options.PluginStructPath.GetFullPath();
        options.IgnoredFileExtensions ??= new List<string>();

        if (options.Verbose)
        {
            var ext_sb = new StringBuilder();
            foreach (var item in options.IgnoredFileExtensions)
                ext_sb.Append($"{item}, ");
            var ignoredFileExtensions = ext_sb
                .ToString()[..(ext_sb.Length - 2 > 0 ? ext_sb.Length - 2 : 0)];

            Console.WriteLine(
                $"""
                {nameof(options.SourcePath)}: {options.SourcePath}
                {nameof(options.OutputPath)}: {options.OutputPath}
                {nameof(options.OutputFileName)}: {options.OutputFileName}
                {nameof(options.PluginStructPath)}: {options.PluginStructPath}
                {nameof(options.LoaderStructPath)}: {options.LoaderStructPath}
                {nameof(options.IgnoredFileExtensions)}: {ignoredFileExtensions}
                """
            );
        }

        var files = new List<string>();
        var sb = new StringBuilder();
        var directories = new Queue<string>();

        var getAllFiles = (string path) =>
        {
            var info = new DirectoryInfo(path);
            foreach (var item in info.GetFiles())
            {
                if (options.IgnoredFileExtensions.Contains(Path.GetExtension(item.FullName)))
                    sb.AppendLine($"Ignored file: {item.FullName}");
                else
                {
                    files.Add(item.FullName);
                    sb.AppendLine($"Found file: {item.FullName}");
                }
            }
            foreach (var item in info.GetDirectories())
                directories.Enqueue(item.FullName);
        };

        getAllFiles(options.SourcePath);

        while (directories.Count > 0)
            getAllFiles(directories.Dequeue());

        if (options.Verbose)
            Console.WriteLine(sb.ToString());

        var encoder = new Encoder(
            files,
            File.ReadAllText(options.LoaderStructPath),
            File.ReadAllText(options.PluginStructPath),
            new()
            {
                Verbose = options.Verbose
            }
        );

        encoder.Encode(
            options.SourcePath,
            options.OutputPath,
            options.OutputFileName
        );

    });
