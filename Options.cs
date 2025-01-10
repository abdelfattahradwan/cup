using CommandLine;

namespace CreateUnityPackage;

internal sealed class Options
{
	[Option('s', "src", Required = true, HelpText = "Root directory path.")]
	public string SourceDirectoryPath { get; set; } = string.Empty;

	[Option('o', "out", Required = true, HelpText = "Output package file path.")]
	public string OutputFilePath { get; set; } = string.Empty;

	[Option('c', "cpr", Required = false, HelpText = "Level of compression. (0 = Optimal, 1 = Fastest, 2 = None, 3 = Smallest)")]
	public byte Compression { get; set; } = 0;

	[Option('e', "exclude", Required = false, HelpText = "List of file/directory glob patterns to exclude.")]
	public IEnumerable<string> ExcludePatterns { get; set; } = [];
}
