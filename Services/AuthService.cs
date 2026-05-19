using Microsoft.EntityFrameworkCore;
using PharmaDesk.Data;
using PharmaDesk.Models;
using PharmaDesk.Helpers;
namespace PharmaDesk.Services;
public class AuthService(PharmaDeskDbContext db, AppSession session, IAuditService audit) : IAuthService
{
    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Username == username || x.Email == username);
        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }
        session.SignIn(user);
        await audit.LogAsync(user.Id, "Login", "Users", user.Id);
        return user;
    }
    public async Task<(bool Success, string Message)> RegisterAsync(string username, string password, string email, string fullName)
    {
        if (!ValidationHelper.IsValidEmail(email)) return (false, "Email invalid.");
        if (!ValidationHelper.IsStrongPassword(password)) return (false, "Parola trebuie sa aiba minim 8 caractere, litera mare si cifra.");
        if (await db.Users.AnyAsync(x => x.Username == username || x.Email == email)) return (false, "Username sau email deja folosit.");
        var roleId = await db.Roles.Where(x => x.Name == "User").Select(x => x.Id).FirstAsync();
        var user = new User
        {
            Username = username.Trim(),
            Email = email.Trim(),
            FullName = fullName.Trim(),
            RoleId = roleId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await audit.LogAsync(user.Id, "Register", "Users", user.Id);
        return (true, "Cont creat cu succes.");
    }
    public async Task ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        var user = await db.Users.FirstAsync(x => x.Id == userId);
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash)) throw new InvalidOperationException("Parola curenta este incorecta.");
        if (!ValidationHelper.IsStrongPassword(newPassword)) throw new InvalidOperationException("Parola noua nu este suficient de puternica.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await db.SaveChangesAsync();
        await audit.LogAsync(userId, "Change password", "Users", userId);
    }
}
