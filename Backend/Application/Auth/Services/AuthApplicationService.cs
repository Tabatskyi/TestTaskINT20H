using TestTaskINT20H.Application.Auth.DTOs;
using TestTaskINT20H.Domain.Auth.Repositories;
using TestTaskINT20H.Domain.Auth.Services;

namespace TestTaskINT20H.Application.Auth.Services;

public sealed class AuthApplicationService(
    IAdminRepository adminRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
{
    public TokenDto? Login(LoginDto dto)
    {
        var admin = adminRepository.FindByUsername(dto.Username);
        if (admin is null || !passwordHasher.Verify(dto.Password, admin.PasswordHash))
            return null;

        var (token, expiresAt) = tokenService.Generate(admin);
        return new TokenDto { Token = token, ExpiresAt = expiresAt };
    }
}
