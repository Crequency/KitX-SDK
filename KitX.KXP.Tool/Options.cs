using CommandLine;

namespace KitX.KXP.Tool;

public class OptionsBase
{
    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }
}

public class Options : OptionsBase
{

}

[Verb("pack", isDefault: true, HelpText = "Pack plugin files into `.kxp` file.")]
public class PackOptions : OptionsBase
{

    [Option('s', "source", Required = true, HelpText = "Source files path.")]
    public string? SourcePath { get; set; }

    [Option('o', "output", HelpText = "Output path of `.kxp` file.")]
    public string OutputPath { get; set; } = "./";

    [Option('n', "output-file-name", HelpText = "Output file name.")]
    public string? OutputFileName { get; set; }

    [Option('l', "loader", HelpText = "Loader struct file path.")]
    public string? LoaderStructPath { get; set; }

    [Option('p', "plugin", HelpText = "Plugin struct file path.")]
    public string? PluginStructPath { get; set; }

    [Option('i', "ignore", Separator = ',', HelpText = "File extensions to ignore.")]
    public IEnumerable<string>? IgnoredFileExtensions { get; set; }
}
