namespace CreateUnityPackage;

public sealed class PathTypeDeductionException(string path) : Exception($"Unable to deduce the path type. Path: '{path}'");
