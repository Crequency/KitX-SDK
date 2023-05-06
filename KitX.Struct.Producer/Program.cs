using CommandLine;
using KitX.Struct.Producer;

var globalOptions = new Options();

Parser.Default.ParseArguments<Options, GenerateOptions>(args)
    .WithParsed<Options>(options => globalOptions = options)
    .WithParsed<GenerateOptions>(options =>
    {
        var path = options.OutputPath;

        if (!Directory.Exists(path))
            throw new ArgumentException($"Path {path} not exists.");

        if (options.Verbose)
        {
            Console.WriteLine($"{nameof(options.GenerateAll)}: {options.GenerateAll}");
            Console.WriteLine(
                $"{nameof(options.GeneratePluginStruct)}: {options.GeneratePluginStruct}"
            );
            Console.WriteLine(
                $"{nameof(options.GenerateLoaderStruct)}: {options.GenerateLoaderStruct}"
            );
        }

        if (options.GeneratePluginStruct || options.GenerateLoaderStruct)
            options.GenerateAll = false;

        if (options.GenerateAll || options.GeneratePluginStruct)
            Producer.ProduceDefaultPluginStruct(path);

        if (options.GenerateAll || options.GenerateLoaderStruct)
            Producer.ProduceDefaultLoaderStruct(path);

        Environment.Exit(0);
    });
