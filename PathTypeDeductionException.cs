using System.Runtime.Serialization;

namespace CreateUnityPackage;

[Serializable]
public sealed class PathTypeDeductionException : Exception
{
	public PathTypeDeductionException(string path) : base($"Unable to deduce the path type. Path: '{path}'")
	{
		Path = path;
	}
	
	private PathTypeDeductionException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
		Path = info.GetString(nameof(Path));
	}

	public string? Path { get; }
	
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		
		info.AddValue(nameof(Path), Path, typeof(string));
	}
}
