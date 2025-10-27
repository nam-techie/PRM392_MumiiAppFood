using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Mumii.Auth.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Mumii.Auth.Infrastructure.Services;

/// <summary>
/// Service gửi email với Neo Brutalism design
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _settings;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        _settings = new EmailSettings
        {
            SmtpServer = configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
            SmtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? "587"),
            SenderEmail = configuration["EmailSettings:SenderEmail"] ?? "",
            SenderName = configuration["EmailSettings:SenderName"] ?? "Mumii Food",
            Password = configuration["EmailSettings:Password"] ?? ""
        };
        
        // Log email configuration (masked password)
        _logger.LogInformation("Email Service initialized - Server: {Server}:{Port}, From: {FromEmail}", 
            _settings.SmtpServer, _settings.SmtpPort, _settings.SenderEmail);
    }

    /// <summary>
    /// Gửi email đặt lại mật khẩu với Neo Brutalism template
    /// </summary>
    public async Task SendPasswordResetEmailAsync(string toEmail, string fullname, string resetToken)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(fullname, toEmail));
            message.Subject = "🍜 Mumii Food - Đặt lại mật khẩu";

            // Neo Brutalism HTML template
            var htmlBody = GenerateNeoBrutalismTemplate(fullname, resetToken);
            message.Body = new TextPart("html")
            {
                Text = htmlBody
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Password reset email sent successfully to: {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to: {Email}", toEmail);
            throw;
        }
    }

    /// <summary>
    /// Implement IEmailService interface - gửi email generic
    /// </summary>
    public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = htmlContent
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to: {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to: {Email}", toEmail);
            throw;
        }
    }

    /// <summary>
    /// Generate Neo Brutalism HTML template
    /// </summary>
    private string GenerateNeoBrutalismTemplate(string fullname, string resetToken)
    {
        return $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Mumii Food - Đặt lại mật khẩu</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            font-family: 'Arial Black', Arial, sans-serif;
            background: linear-gradient(135deg, #FF6B35 0%, #F7931E 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
        }}
        .container {{
            max-width: 600px;
            margin: 20px;
            background: #FFFFFF;
            border: 4px solid #000000;
            box-shadow: 8px 8px 0px rgba(0,0,0,1);
            border-radius: 0;
        }}
        .header {{
            background: #FF6B35;
            color: #FFFFFF;
            padding: 30px;
            text-align: center;
            border-bottom: 4px solid #000000;
        }}
        .header h1 {{
            margin: 0;
            font-size: 32px;
            font-weight: 900;
            text-shadow: 2px 2px 0px #000000;
        }}
        .content {{
            padding: 40px;
            color: #000000;
        }}
        .greeting {{
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 20px;
        }}
        .message {{
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 30px;
        }}
        .otp-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .otp-box {{
            display: inline-block;
            background: #FFD23F;
            border: 4px solid #000000;
            padding: 20px 40px;
            font-size: 48px;
            font-weight: 900;
            letter-spacing: 8px;
            color: #000000;
            box-shadow: 4px 4px 0px rgba(0,0,0,1);
            margin: 20px 0;
        }}
        .button {{
            display: inline-block;
            background: #000000;
            color: #FFFFFF;
            padding: 15px 30px;
            text-decoration: none;
            font-weight: bold;
            font-size: 16px;
            border: 4px solid #000000;
            box-shadow: 4px 4px 0px rgba(0,0,0,1);
            transition: all 0.2s;
            margin: 20px 0;
        }}
        .button:hover {{
            transform: translate(2px, 2px);
            box-shadow: 2px 2px 0px rgba(0,0,0,1);
        }}
        .footer {{
            background: #F8F8F8;
            padding: 20px;
            border-top: 4px solid #000000;
            text-align: center;
            font-size: 14px;
            color: #666666;
        }}
        .warning {{
            background: #FFF3CD;
            border: 2px solid #FFC107;
            padding: 15px;
            margin: 20px 0;
            font-weight: bold;
        }}
        @media (max-width: 600px) {{
            .container {{
                margin: 10px;
            }}
            .content {{
                padding: 20px;
            }}
            .otp-box {{
                font-size: 36px;
                padding: 15px 30px;
            }}
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🍜 Mumii Food</h1>
        </div>
        
        <div class=""content"">
            <div class=""greeting"">
                Xin chào {fullname},
            </div>
            
            <div class=""message"">
                Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình tại <strong>Mumii Food</strong>.
                <br><br>
                Vui lòng nhập mã OTP 6 số dưới đây để xác thực:
            </div>
            
            <div class=""otp-container"">
                <div class=""otp-box"">{resetToken}</div>
            </div>
            
            <div class=""warning"">
                ⚠️ Mã OTP này chỉ có hiệu lực trong 1 giờ. Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.
            </div>
            
            <div style=""text-align: center; margin: 30px 0;"">
                <a href=""#"" class=""button"">Nhập OTP Và Đặt Lại Mật Khẩu</a>
            </div>
            
            <div class=""message"">
                Cảm ơn bạn đã lựa chọn <strong>Mumii Food</strong> - Nơi kết nối tình yêu ẩm thực! 🍜
            </div>
        </div>
        
        <div class=""footer"">
            <p><strong>Mumii Food</strong> - Kết nối tình yêu ẩm thực</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Email settings configuration
    /// </summary>
    private class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
