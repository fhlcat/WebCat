using System.Text.Json.Serialization;
using WebCat.Process.Struct;

namespace WebCat;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, IncludeFields = true)]
[JsonSerializable(typeof(ProcessRequest))]
[JsonSerializable(typeof(Work.WorkRecord))]
public partial class JsonSourceGenerationContext: JsonSerializerContext;