using System.Text.Json.Serialization;

namespace TestTaskINT20H.Application.Auth.DTOs;

public sealed record LoginDto
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed record TokenDto
{
    public string Token { get; init; } = string.Empty;

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; init; }
}
