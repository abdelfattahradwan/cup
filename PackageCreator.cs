using System.Collections.Immutable;
using System.IO.Compression;

namespace CreateUnityPackage;

internal static class PackageCreator
{
    private static readonly EnumerationOptions FileSystemEnumerationOptions = new()
    {
        RecurseSubdirectories = false,

        ReturnSpecialDirectories = false,
    };

    private static async Task<Asset?> GetAssetAsync(string filePath)
    {
        string metaFilePath = $"{filePath}.meta";

        if (!File.Exists(metaFilePath)) return null;

        using StreamReader reader = File.OpenText(metaFilePath);

        while (true)
        {
            string? line = await reader.ReadLineAsync();

            if (line is null) break;

            if (!line.StartsWith("guid:")) continue;

            string guid = line[5..].Trim();

            int assetsIndex = filePath.IndexOf("Assets", StringComparison.Ordinal);

            string relativeFilePath = filePath[assetsIndex..].Replace('\\', '/');

            return string.IsNullOrWhiteSpace(guid) ? null : new Asset(filePath, relativeFilePath, metaFilePath, guid);
        }

        return null;
    }

    private static async Task<List<Asset>> GetAssetsAsync(string sourceDirectoryPath, IReadOnlySet<string> ignoredPaths)
    {
        List<Asset> assets = [];

        foreach (string path in Directory.EnumerateFileSystemEntries(sourceDirectoryPath, "*", FileSystemEnumerationOptions))
        {
            if (ignoredPaths.Contains(path)) continue;

            if (Directory.Exists(path))
            {
                assets.AddRange(await GetAssetsAsync(path, ignoredPaths));
            }
            else if (File.Exists(path))
            {
                Asset? asset = await GetAssetAsync(path);

                if (asset is not null) assets.Add(asset.Value);
            }
            else
            {
                throw new PathTypeDeductionException(path);
            }
        }

        return assets;
    }

    public static async Task CreatePackageFromDirectoryAsync(Options options)
    {
        ImmutableHashSet<string> ignoredPaths = options.IgnoredPaths.Select(Path.GetFullPath).ToImmutableHashSet();

        List<Asset> assets = await GetAssetsAsync(Environment.CurrentDirectory, ignoredPaths);

        if (assets.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            await Console.Out.WriteLineAsync("No assets found. Exiting...");

            Console.ResetColor();

            return;
        }

        string temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            foreach (Asset asset in assets)
            {
                DirectoryInfo temporaryAssetDirectory = Directory.CreateDirectory(Path.Combine(temporaryDirectoryPath, asset.Guid));

                File.Copy(asset.FilePath, Path.Combine(temporaryAssetDirectory.FullName, "asset"));

                File.Copy(asset.MetaFilePath, Path.Combine(temporaryAssetDirectory.FullName, "asset.meta"));

                await File.WriteAllTextAsync(Path.Combine(temporaryAssetDirectory.FullName, "pathname"), asset.RelativeFilePath);
            }

            if (File.Exists(options.OutputFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;

                await Console.Out.WriteLineAsync($"Deleting existing file: '{options.OutputFilePath}'.");

                Console.ResetColor();

                File.Delete(options.OutputFilePath);
            }

            ZipFile.CreateFromDirectory(temporaryDirectoryPath, options.OutputFilePath, (CompressionLevel)options.Compression, false);

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
        finally
        {
            if (Directory.Exists(temporaryDirectoryPath)) Directory.Delete(temporaryDirectoryPath, true);
        }
    }
}
