namespace Mumii.Auth.Domain.Interfaces;

/// <summary>
/// Interface cho email service
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi email đặt lại mật khẩu
    /// </summary>
    /// <param name="toEmail">Email người nhận</param>
    /// <param name="fullname">Tên người nhận</param>
    /// <param name="resetToken">Token đặt lại mật khẩu</param>
    /// <returns>Task</returns>
    Task SendPasswordResetEmailAsync(string toEmail, string fullname, string resetToken);
}
