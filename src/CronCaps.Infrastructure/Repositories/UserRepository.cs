using CronCaps.Domain.Entities;
using CronCaps.Domain.Enums;
using CronCaps.Domain.Interfaces;
using CronCaps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CronCaps.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CronCapsDbContext _context;

    public UserRepository(CronCapsDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Team)
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Team)
            .Where(u => u.TeamId == teamId && u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        user.Deactivate();
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == id && u.IsActive, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Where(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.CountAsync(u => u.IsActive, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.Name.ToLower() == username.ToLower() && u.IsActive, cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // For now, we'll implement a simple approach - in a real app, you'd store refresh tokens
        // This is a placeholder implementation
        return null;
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Team)
            .Where(u => u.Role == role && u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var search = searchTerm.ToLower();
        return await _context.Users
            .Include(u => u.Team)
            .Where(u => u.IsActive &&
                       (u.Name.ToLower().Contains(search) ||
                        u.Email.ToLower().Contains(search)))
            .OrderBy(u => u.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.Where(u => u.Name.ToLower() == username.ToLower() && u.IsActive);

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        return !await EmailExistsAsync(email, excludeUserId, cancellationToken);
    }
}