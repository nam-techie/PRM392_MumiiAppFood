namespace Mumii.Auth.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlContent);
    Task SendPasswordResetEmailAsync(string toEmail, string fullname, string resetToken);
}