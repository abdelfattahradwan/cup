using Microsoft.Extensions.FileSystemGlobbing;
using System.IO.Compression;

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

            int assetsIndex = filePath.IndexOf("Assets", StringComparison.Ordinal);

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
        string temporaryDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

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

            foreach (Asset asset in assets)
            {
                string temporaryAssetDirectoryPath = Path.Combine(temporaryDirectoryPath, asset.Guid);

                Directory.CreateDirectory(temporaryAssetDirectoryPath);

                File.Copy(asset.FilePath, Path.Combine(temporaryAssetDirectoryPath, "asset"));

                File.Copy(asset.MetaFilePath, Path.Combine(temporaryAssetDirectoryPath, "asset.meta"));

                await File.WriteAllTextAsync(Path.Combine(temporaryAssetDirectoryPath, "pathname"), asset.RelativeFilePath);
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
