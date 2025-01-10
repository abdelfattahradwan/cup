using Microsoft.Extensions.FileSystemGlobbing;
using System.IO.Compression;
using System.Text;

namespace CreateUnityPackage;

internal static class PackageCreator
{
	private static async Task<Asset?> GetAssetAsync(string metaFilePath)
	{
		string? metaFileDirectoryPath = Path.GetDirectoryName(metaFilePath);

		ArgumentException.ThrowIfNullOrWhiteSpace(metaFileDirectoryPath);

		string filePath = Path.Combine(metaFileDirectoryPath, Path.GetFileNameWithoutExtension(metaFilePath));

		if (Directory.Exists(filePath)) return null;

		using StreamReader reader = File.OpenText(metaFilePath);

		while (true)
		{
			string? line = await reader.ReadLineAsync();

			if (line is null) break;

			if (!line.StartsWith("guid:")) continue;

			string guid = line[5..].Trim();

			if (string.IsNullOrWhiteSpace(guid)) throw new InvalidOperationException($"The GUID in '{metaFilePath}' is empty or whitespace.");

			int assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);

			string relativeFilePath = filePath[assetsIndex..].Replace('\\', '/');

			return new Asset(filePath, relativeFilePath, metaFilePath, guid);
		}

		throw new InvalidOperationException($"No GUID found in '{metaFilePath}'.");
	}

	private static async Task<List<Asset>> GetAssetsAsync(string sourceDirectoryPath, IEnumerable<string> excludePatterns)
	{
		List<Asset> assets = [];

		Matcher matcher = new();

		matcher.AddInclude("**/*.meta");

		matcher.AddExcludePatterns(excludePatterns);

		foreach (string metaFilePath in matcher.GetResultsInFullPath(sourceDirectoryPath))
		{
			Asset? asset = await GetAssetAsync(metaFilePath);

			if (asset is not null) assets.Add(asset.Value);
		}

		return assets;
	}

	public static async Task CreatePackageFromDirectoryAsync(Options options)
	{
		try
		{
			List<Asset> assets = await GetAssetsAsync(Environment.CurrentDirectory, options.ExcludePatterns);

			if (assets.Count == 0)
			{
				Console.ForegroundColor = ConsoleColor.White;

				await Console.Out.WriteLineAsync("No assets found. Exiting...");

				Console.ResetColor();

				return;
			}

			if (File.Exists(options.OutputFilePath))
			{
				Console.ForegroundColor = ConsoleColor.Yellow;

				await Console.Out.WriteLineAsync($"Deleting existing file: '{options.OutputFilePath}'.");

				Console.ResetColor();

				File.Delete(options.OutputFilePath);
			}

			CompressionLevel compressionLevel = (CompressionLevel)options.Compression;

			using (ZipArchive zipArchive = ZipFile.Open(options.OutputFilePath, ZipArchiveMode.Create))
			{
				foreach (Asset asset in assets)
				{
					zipArchive.CreateEntryFromFile(asset.FilePath, $"{asset.Guid}/asset", compressionLevel);

					zipArchive.CreateEntryFromFile(asset.MetaFilePath, $"{asset.Guid}/asset.meta", compressionLevel);

					ZipArchiveEntry pathnameEntry = zipArchive.CreateEntry($"{asset.Guid}/pathname", compressionLevel);

					await using Stream pathnameEntryStream = pathnameEntry.Open();

					byte[] relativeFilePathBytes = Encoding.UTF8.GetBytes(asset.RelativeFilePath);

					await pathnameEntryStream.WriteAsync(relativeFilePathBytes);
				}
			}

			Console.ForegroundColor = ConsoleColor.Green;

			await Console.Out.WriteLineAsync($"Package created: '{options.OutputFilePath}'.");

			Console.ResetColor();
		}
		catch (Exception exception)
		{
			Console.ForegroundColor = ConsoleColor.Red;

			await Console.Error.WriteLineAsync("Failed to create package.");

			await Console.Error.WriteLineAsync(exception.ToString());

			Console.ResetColor();
		}
	}
}
