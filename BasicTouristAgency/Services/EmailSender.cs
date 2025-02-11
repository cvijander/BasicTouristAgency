using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BasicTouristAgency.Services
{
    public class EmailSender : IEmailSender
    {

        private readonly ILogger<EmailSender> _logger;
        private readonly IConfiguration _configuration;


        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }


        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                // laoa email setting form configuration
                var emailSettings = _configuration.GetSection("EmailSettings");
                var host = emailSettings["Host"];
                var port = int.Parse(emailSettings["Port"]);
                var enableSSL = bool.Parse(emailSettings["EnableSSL"]);
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];
                var from = emailSettings["From"];

                // confuigure smt client
                var smtpClient = new SmtpClient(host)
                {
                    Port = port,
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSSL
                };

                //create the email messafge

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(from),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true  // allows senthg html emails
                };

                mailMessage.To.Add(email);

                //send the emil
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email set to {email} with subject {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failder to send email to {email} : {ex.Message}");
                throw;
            }

            // await Task.CompletedTask;
        }
    }
}
