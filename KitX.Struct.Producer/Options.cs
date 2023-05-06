using CommandLine;

namespace KitX.Struct.Producer;

public class OptionsBase
{
    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }
}

public class Options : OptionsBase
{

}

[Verb(
    "generate",
    aliases: new string[] { "gen" },
    isDefault: true,
    HelpText = "Generate KitX (Loader/Plugin) Struct."
)]
public class GenerateOptions : OptionsBase
{
    [Option('a', "all", Group = "templates", Default = true, HelpText = "Generate All.")]
    public bool GenerateAll { get; set; }

    [Option('l', "loader", Group = "templates", HelpText = "Generate Loader Struct.")]
    public bool GenerateLoaderStruct { get; set; }

    [Option('p', "plugin", Group = "templates", HelpText = "Generate Plugin Struct.")]
    public bool GeneratePluginStruct { get; set; }

    [Option('o', "output", Required = false, HelpText = "Output path.")]
    public string OutputPath { get; set; } = "./";
}
