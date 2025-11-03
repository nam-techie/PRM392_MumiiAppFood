using Microsoft.EntityFrameworkCore;
using Mumii.Auth.Domain.Entities;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Data;

namespace Mumii.Auth.Infrastructure.Repositories;

/// <summary>
/// Implementation của IAccountRepository
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly AuthDbContext _context;

    public AccountRepository(AuthDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tìm tài khoản theo email
    /// </summary>
    public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == email.ToLower(), cancellationToken);
    }

    /// <summary>
    /// Tìm tài khoản theo ID
    /// </summary>
    public async Task<Account?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    /// <summary>
    /// Thêm tài khoản mới
    /// </summary>
    public async Task<Account> AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
        return account;
    }

    /// <summary>
    /// Cập nhật tài khoản
    /// </summary>
    public async Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        return await Task.FromResult(account);
    }

    /// <summary>
    /// Kiểm tra email đã tồn tại chưa
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.Email == email.ToLower(), cancellationToken);
    }

    /// <summary>
    /// Lưu thay đổi
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
