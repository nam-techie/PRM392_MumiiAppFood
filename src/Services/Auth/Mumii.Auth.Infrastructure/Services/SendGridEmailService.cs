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

	public async Task SendPasswordResetEmailAsync(string toEmail, string fullname, string resetToken)
	{
		var subject = "🍜 Mumii Food - Đặt lại mật khẩu";
		var htmlContent = $@"
			<h1>Yêu cầu đặt lại mật khẩu</h1>
			<p>Xin chào {fullname},</p>
			<p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
			<p>Mã OTP của bạn là: <strong>{resetToken}</strong></p>
			<p>Mã này sẽ hết hạn sau 10 phút. Vui lòng không chia sẻ mã này với bất kỳ ai.</p>
			<p>Trân trọng,<br>Đội ngũ Mumii App</p>";

		await SendEmailAsync(toEmail, subject, htmlContent);
	}
}