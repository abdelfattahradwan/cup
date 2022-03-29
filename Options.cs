using CommandLine;

namespace CreateUnityPackage;

internal sealed class Options
{
	[Option('s', "src", Required = true, HelpText = "Root directory path.")]
	public string SourceDirectoryPath { get; set; } = string.Empty;

	[Option('t', "tmp", Required = false, HelpText = "Temporary working directory path.")]
	public string TemporaryDirectoryPath { get; set; } = Path.GetTempPath();

	[Option('o', "out", Required = true, HelpText = "Output package file path.")]
	public string OutputFilePath { get; set; } = string.Empty;

	[Option('c', "cpr", Required = false, HelpText = "Level of compression. (0 = Optimal, 1 = Fastest, 2 = None, 3 = Smallest)")]
	public byte Compression { get; set; } = 0;

	[Option("ignore", Required = false, HelpText = "List of file/directory paths to ignore.")]
	public IEnumerable<string> IgnoredPaths { get; set; } = Enumerable.Empty<string>();
}
