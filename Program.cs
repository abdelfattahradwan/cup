using CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CreateUnityPackage;

internal static class Program
{
	private const string IgnoreFileName = ".cupignore";

	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Options))]
	public static void Main(string[] args)
	{
		Parser.Default.ParseArguments<Options>(args).WithParsed(HandleParsed);
	}

	private static void HandleParsed(Options options)
	{
		Environment.CurrentDirectory = Path.GetFullPath(options.SourceDirectoryPath);

		if (File.Exists(IgnoreFileName))
		{
			try
			{
				using Stream stream = File.OpenRead(IgnoreFileName);

				string[] excludePatterns = JsonSerializer.Deserialize(stream, ProgramJsonSerializerContext.Default.StringArray) ?? [];

				options.ExcludePatterns = options.ExcludePatterns.Concat(excludePatterns);
			}
			catch (Exception exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;

				Console.Error.WriteLine($"Failed to load the .cupignore file at '{Path.GetFullPath(IgnoreFileName)}'.");

				Console.Error.WriteLine(exception.ToString());

				Console.ResetColor();

				Console.Out.WriteLine("Do you want to continue? (y/n)");

				string? input = Console.In.ReadLine();

				if (string.Equals(input, "y", StringComparison.OrdinalIgnoreCase))
				{
					Console.Out.WriteLine("Continuing...");
				}
				else
				{
					Console.Out.WriteLine("Exiting...");

					return;
				}
			}
		}

		PackageCreator.CreatePackageFromDirectory(options);
	}
}
