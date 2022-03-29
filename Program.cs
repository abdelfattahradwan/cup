using CommandLine;
using System.Text.Json;

namespace CreateUnityPackage;

internal static class Program
{
	public static async Task Main(string[] args)
	{
		await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(Execute);
	}

	private static async Task Execute(Options options)
	{
		Environment.CurrentDirectory = Path.GetFullPath(options.SourceDirectoryPath);

		Console.WriteLine(Environment.CurrentDirectory);

		if (File.Exists(".cupignore") && JsonSerializer.Deserialize<IEnumerable<string>>(await File.ReadAllTextAsync(".cupignore")) is not null and IEnumerable<string> ignorePaths)
		{
			options.IgnoredPaths = options.IgnoredPaths.Concat(ignorePaths);
		}

		await PackageCreator.CreatePackageFromDirectory(options);
	}
}
