using CommandLine;
using System.Text.Json;

namespace CreateUnityPackage;

internal static class Program
{
	private const string IgnoreFileName = ".cupignore";

	public static async Task Main(string[] args)
	{
		await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(HandleParsedAsync);
	}

	private static async Task HandleParsedAsync(Options options)
	{
		Environment.CurrentDirectory = Path.GetFullPath(options.SourceDirectoryPath);

		if (File.Exists(IgnoreFileName))
		{
			try
			{
				if (JsonSerializer.Deserialize<IEnumerable<string>>(await File.ReadAllTextAsync(IgnoreFileName)) is { } extraIgnoredPaths) options.IgnoredPaths = options.IgnoredPaths.Concat(extraIgnoredPaths);
			}
			catch (Exception exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;

				Console.WriteLine($"Failed to load the .cupignore file at '{Path.GetFullPath(IgnoreFileName)}'.");

				Console.WriteLine(exception.ToString());

				Console.ResetColor();

				Console.WriteLine("Do you want to continue? (y/n)");

				if (string.Equals(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase))
				{
					Console.WriteLine("Continuing...");
				}
				else
				{
					Console.WriteLine("Exiting...");

					return;
				}
			}
		}

		await PackageCreator.CreatePackageFromDirectory(options);
	}
}
