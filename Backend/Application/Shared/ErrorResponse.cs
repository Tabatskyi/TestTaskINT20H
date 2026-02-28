using System.Text.Json.Serialization;

namespace TestTaskINT20H.Application.Shared;

public sealed record ErrorResponse
{
    public string Error { get; init; } = string.Empty;

    [JsonPropertyName("skipped_rows")]
    public List<int>? SkippedRows { get; init; }
}
