using System.Text.Json;
using System.Text.Json.Serialization;

static class Json
{
    public static JsonSerializerOptions Defaults { get; }
        = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
}
