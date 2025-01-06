using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PMS.Shared.Models;
using PMS.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Shared.Services
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync(MailMessage message);
        bool SendEmail(MailMessage message);
    }
    public class EmailSender:IEmailSender
    {

        readonly IOptions<AppSetting> _config;
        private SmtpClient _smtpClient;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<AppSetting> config, ILogger<EmailSender> logger)
        {
            _config = config;
            _smtpClient = new SmtpClient(_config.Value.SmtpHost,_config.Value.SmtpPort);

            string enctyptionKey = _config.Value.ConfigEncrytionKey;
            string smtpUser = _config.Value.SmtpUser, smtpPassword = _config.Value.SmtpPassword;
            if (!string.IsNullOrWhiteSpace(smtpUser))
                smtpUser = PMSEncryption.Decrypt(smtpUser, enctyptionKey);            
            if (!string.IsNullOrWhiteSpace(smtpPassword))
                smtpPassword = PMSEncryption.Decrypt(smtpPassword, enctyptionKey);

            if (string.IsNullOrWhiteSpace(smtpUser))
                _smtpClient.UseDefaultCredentials = true;
            else
            {
                _smtpClient.UseDefaultCredentials = false;
                _smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPassword);
            }

            _smtpClient.EnableSsl = _config.Value.SmtpUseSsl;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(MailMessage message)
        {
            bool result = false;            
            message.From = new MailAddress(_config.Value.SmtpSenderAddress, _config.Value.SmtpSenderName);
            try
            {
                _logger.LogInformation("[Start]send email to " + message.To[0].Address);
                await _smtpClient.SendMailAsync(message);
                _logger.LogInformation("[Finish]send email to " + message.To[0].Address);
                result = true;
            }
            catch(Exception ex) 
            {
                _logger.LogError("[Error]send email to " + message.To[0].Address + ":" + ex.Message);
            }
            return result;
        }


        public bool SendEmail(MailMessage message)
        {
            bool result = false;
            message.From = new MailAddress(_config.Value.SmtpSenderAddress, _config.Value.SmtpSenderName);
            try
            {
                _logger.LogInformation("[Start]send email to " + message.To[0].Address);
                _smtpClient.Send(message);
                _logger.LogInformation("[Finish]send email to " + message.To[0].Address);
                result = true;
            }
            catch(Exception ex) 
            {
                _logger.LogError("[Error]send email to " + message.To[0].Address + ":" + ex.Message);
            }
            return result;
        }
    }
}
