using CommandLine;

namespace SyncCodes;

public class CommandLineOptions
{
    [Option('d', "directory", HelpText = "Directory to synchronize")]
    public string? WorkBase { get; set; }

    [Option('j', "job", HelpText = "Job to do (provide-codes, fetch-codes")]
    public Jobs Job { get; set; } = Jobs.ProvideCodes;

    [Option('v', "verbose", HelpText = "Display verbose output")]
    public bool Verbose { get; set; }
}
