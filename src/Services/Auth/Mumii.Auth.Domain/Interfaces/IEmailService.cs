namespace Mumii.Auth.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlContent);
}