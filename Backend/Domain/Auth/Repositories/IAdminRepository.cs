using TestTaskINT20H.Domain.Auth.Entities;

namespace TestTaskINT20H.Domain.Auth.Repositories;

public interface IAdminRepository
{
    Admin? FindByUsername(string username);
    bool HasAny();
    void Add(Admin admin);
    void SaveChanges();
}
