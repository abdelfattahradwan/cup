using CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CreateUnityPackage;

internal static class Program
{
	private const string IgnoreFileName = ".cupignore";

	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Options))]
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

				string[] excludePatterns = await JsonSerializer.DeserializeAsync(stream, ProgramJsonSerializerContext.Default.StringArray) ?? [];

				options.ExcludePatterns = options.ExcludePatterns.Concat(excludePatterns);
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
					await Console.Out.WriteLineAsync("Continuing...");
				}
				else
				{
					await Console.Out.WriteLineAsync("Exiting...");

					return;
				}
			}
		}

		await PackageCreator.CreatePackageFromDirectoryAsync(options);
	}
}
