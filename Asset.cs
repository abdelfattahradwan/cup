namespace CreateUnityPackage;

internal readonly record struct Asset(string FilePath, string RelativeFilePath, string MetaFilePath, string Guid);
