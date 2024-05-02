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
				await using Stream stream = File.OpenRead(IgnoreFileName);

				IEnumerable<string> excluded = await JsonSerializer.DeserializeAsync<IEnumerable<string>>(stream) ?? Enumerable.Empty<string>();
				
				options.IgnoredPaths = options.IgnoredPaths.Concat(excluded);
			}
			catch (Exception exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;

				await Console.Error.WriteLineAsync($"Failed to load the .cupignore file at '{Path.GetFullPath(IgnoreFileName)}'.");

				await Console.Error.WriteLineAsync(exception.ToString());

				Console.ResetColor();

				await Console.Out.WriteLineAsync("Do you want to continue? (y/n)");
				
				string? input = await Console.In.ReadLineAsync();

				if (string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
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

		await PackageCreator.CreatePackageFromDirectoryAsync(options);
	}
}
