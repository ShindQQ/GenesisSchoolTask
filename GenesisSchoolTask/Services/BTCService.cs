﻿using GenesisSchoolTask.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GenesisSchoolTask.Services
{
    public class BTCService : IBTCService
    {
        private readonly IConfiguration _configuration;

        public BTCService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<RateDto?> GetRate(string connection)
        {
            using var client = new HttpClient();

            var content = await client.GetFromJsonAsync<RateDto>(connection);

            if (content == null)
            {
                return null;
            }

            if (!content.data.TryGetValue("BTC", out string? currency))
            {
                return null;
            }

            return content;
        }

        public async Task<bool> SignEmailUp(string pathToFile, string email)
        {
            string? line;
            bool check = false;

            using (var reader = new StreamReader(pathToFile))
            {
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line == email)
                    {
                        check = true;
                        break;
                    }
                }
            }

            if (line == null || !check)
            {
                using (var writer = new StreamWriter(pathToFile, true))
                {
                    await writer.WriteLineAsync(email);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public async void SendEmails(string pathToFile, string connection, string currency)
        {
            string? line;
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Genesis BTC rate", _configuration.GetSection("EmailUserName").Value));
            var bodyBuilder = new BodyBuilder();

            using (var client = new SmtpClient())
            {
                client.Connect(_configuration.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
                client.Authenticate(_configuration.GetSection("EmailUserName").Value, _configuration.GetSection("EmailPassword").Value);

                using (var reader = new StreamReader(pathToFile))
                {
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        emailMessage.To.Add(new MailboxAddress("To", line));
                        emailMessage.Subject = "BTC-UAH rate";
                        bodyBuilder.HtmlBody = !String.IsNullOrEmpty(currency) ? $"1 BTC costs: {currency} {connection.Substring(connection.Length - 3)}" : $"Unable to send current BTC in {connection.Substring(connection.Length - 3)} rate";
                        emailMessage.Body = bodyBuilder.ToMessageBody();
                    }

                    await client.SendAsync(emailMessage);
                }

                client.Disconnect(true);
            }
        }
    }
}