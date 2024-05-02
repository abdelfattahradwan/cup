using System.Text.Json.Serialization;

namespace CreateUnityPackage;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(string[]))]
internal partial class ProgramJsonSerializerContext : JsonSerializerContext;
