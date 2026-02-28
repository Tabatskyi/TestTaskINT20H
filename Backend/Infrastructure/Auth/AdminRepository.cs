using TestTaskINT20H.Domain.Auth.Entities;
using TestTaskINT20H.Domain.Auth.Repositories;
using TestTaskINT20H.Infrastructure.Persistence;

namespace TestTaskINT20H.Infrastructure.Auth;

public sealed class AdminRepository(AdminDbContext dbContext) : IAdminRepository
{
    public Admin? FindByUsername(string username)
        => dbContext.Admins.FirstOrDefault(a => a.Username == username);

    public bool HasAny() => dbContext.Admins.Any();

    public void Add(Admin admin) => dbContext.Admins.Add(admin);

    public void SaveChanges() => dbContext.SaveChanges();
}
