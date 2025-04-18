using Microsoft.Extensions.FileSystemGlobbing;
using System.Buffers;
using System.IO.Compression;
using System.Text;

namespace CreateUnityPackage;

internal static class PackageCreator
{
	private static Asset? GetAsset(string metaFilePath)
	{
		string? metaFileDirectoryPath = Path.GetDirectoryName(metaFilePath);

		ArgumentException.ThrowIfNullOrWhiteSpace(metaFileDirectoryPath);

		string filePath = Path.Combine(metaFileDirectoryPath, Path.GetFileNameWithoutExtension(metaFilePath));

		if (Directory.Exists(filePath)) return null;

		using StreamReader reader = File.OpenText(metaFilePath);

		while (true)
		{
			string? line = reader.ReadLine();

			if (line is null) break;

			if (!line.StartsWith("guid:", StringComparison.OrdinalIgnoreCase)) continue;

			ReadOnlySpan<char> guid = line.AsSpan(5).Trim();

			if (guid.IsEmpty) throw new InvalidOperationException($"The GUID in '{metaFilePath}' is empty or whitespace.");

			int assetsIndex = filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);

			string relativeFilePath = filePath[assetsIndex..].Replace('\\', '/');

			return new Asset(filePath, relativeFilePath, metaFilePath, new string(guid));
		}

		throw new InvalidOperationException($"No GUID found in '{metaFilePath}'.");
	}

	private static List<Asset> GetAssets(string sourceDirectoryPath, IEnumerable<string> excludePatterns)
	{
		List<Asset> assets = [];

		Matcher matcher = new();

		matcher.AddInclude("**/*.meta");

		matcher.AddExcludePatterns(excludePatterns);

		foreach (string metaFilePath in matcher.GetResultsInFullPath(sourceDirectoryPath))
		{
			Asset? asset = GetAsset(metaFilePath);

			if (asset is not null) assets.Add(asset.Value);
		}

		return assets;
	}

	public static void CreatePackageFromDirectory(Options options)
	{
		try
		{
			Console.Out.WriteLine("Scanning for assets...");

			List<Asset> assets = GetAssets(Environment.CurrentDirectory, options.ExcludePatterns);

			Console.Out.WriteLine($"Found ${assets} asset(s).");

			if (assets.Count == 0)
			{
				Console.Out.WriteLine("No assets found. Exiting...");

				return;
			}

			if (File.Exists(options.OutputFilePath))
			{
				Console.ForegroundColor = ConsoleColor.Yellow;

				Console.Out.WriteLine($"Deleting existing file: '{options.OutputFilePath}'.");

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

					using Stream pathnameEntryStream = pathnameEntry.Open();

					byte[] relativeFilePathBytes = ArrayPool<byte>.Shared.Rent(512);

					try
					{
						int length = Encoding.UTF8.GetBytes(asset.RelativeFilePath, relativeFilePathBytes);

						pathnameEntryStream.Write(relativeFilePathBytes, 0, length);
					}
					finally
					{
						ArrayPool<byte>.Shared.Return(relativeFilePathBytes);
					}
				}
			}

			Console.ForegroundColor = ConsoleColor.Green;

			Console.Out.WriteLine($"Package created: '{options.OutputFilePath}'.");

			Console.ResetColor();
		}
		catch (Exception exception)
		{
			Console.ForegroundColor = ConsoleColor.Red;

			Console.Error.WriteLine("Failed to create package.");

			Console.Error.WriteLine(exception.ToString());

			Console.ResetColor();
		}
	}
}
