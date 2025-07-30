using System.Text.Json.Serialization;
using WebCat;

namespace WebCatCli;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, IncludeFields = true)]
[JsonSerializable(typeof(string[][]))]
[JsonSerializable(typeof(Main.MainOptions))]
[JsonSerializable(typeof(Process.ProcessOptions))]
[JsonSerializable(typeof(Main.MainResult))]
public partial class LoggingJsonSourceGenerationContext : JsonSerializerContext;