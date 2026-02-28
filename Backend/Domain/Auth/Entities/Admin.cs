namespace TestTaskINT20H.Domain.Auth.Entities;

public sealed class Admin
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    private Admin() { } // EF Core

    public static Admin Create(string username, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new Admin
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHash
        };
    }
}
