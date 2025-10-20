using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mumii.Auth.Domain.Interfaces;
using Mumii.Auth.Infrastructure.Settings;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Mumii.Auth.Infrastructure.Services;

public class SendGridEmailService : IEmailService
{
	private readonly Mumii.Auth.Infrastructure.Settings.MailSettings _mailSettings;
	private readonly ILogger<SendGridEmailService> _logger;

	public SendGridEmailService(IOptions<Mumii.Auth.Infrastructure.Settings.MailSettings> mailSettings, ILogger<SendGridEmailService> logger)
	{
		_mailSettings = mailSettings.Value;
		_logger = logger;
	}

	public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
	{
		if (string.IsNullOrEmpty(_mailSettings.ApiKey))
		{
			_logger.LogError("SendGrid API Key is not configured.");
			return;
		}

		var client = new SendGridClient(_mailSettings.ApiKey);
		var from = new EmailAddress(_mailSettings.FromEmail, _mailSettings.FromName);
		var to = new EmailAddress(toEmail);
		var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);

		var response = await client.SendEmailAsync(msg);

		if (response.IsSuccessStatusCode)
		{
			_logger.LogInformation("Email sent successfully to {Email}", toEmail);
		}
		else
		{
			_logger.LogError("Failed to send email to {Email}. Status Code: {StatusCode}, Body: {Body}",
				toEmail, response.StatusCode, await response.Body.ReadAsStringAsync());
		}
	}
}