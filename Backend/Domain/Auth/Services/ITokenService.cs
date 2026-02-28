using TestTaskINT20H.Domain.Auth.Entities;

namespace TestTaskINT20H.Domain.Auth.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) Generate(Admin admin);
}
