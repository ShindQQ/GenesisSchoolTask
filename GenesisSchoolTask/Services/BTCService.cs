using GenesisSchoolTask.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GenesisSchoolTask.Services
{
    /// <summary>
    /// Service for getting current btc rate in different currencies
    /// </summary>
    public class BTCService : IBTCService
    {
        private readonly IConfiguration _configuration;
        private readonly string _pathToFile;
        private readonly string _connection;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">Receiving configuration to use appsettings.json</param>
        public BTCService(IConfiguration configuration)
        {
            _configuration = configuration;
            _pathToFile = _configuration.GetSection("PathToFile").Value;
            _connection = _configuration.GetSection("ConnectionString").Value;
        }

        /// <summary>
        /// Receiving rate of btc
        /// </summary>
        /// <returns>RateDto which has dictionary with cryptocurrency infos</returns>
        public async Task<RateDto?> GetRate()
        {
            using var client = new HttpClient();

            var content = await client.GetFromJsonAsync<RateDto>(_connection);

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

        /// <summary>
        /// Signing email on notification of rate
        /// </summary>
        /// <param name="email">Email which we will add on notification</param>
        /// <returns>True if everything is ok, and false if we had such email if our file</returns>
        public async Task<bool> SignEmailUp(string email)
        {
            string? line;
            bool check = false;

            using (var reader = new StreamReader(_pathToFile))
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
                using (var writer = new StreamWriter(_pathToFile, true))
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

        /// <summary>
        /// Sending emails to all users
        /// </summary>
        /// <param name="currency">Current rate of needed currency</param>
        /// <param name="currencyName">Current name of needed currency</param>
        public async void SendEmails(string currency, string currencyName)
        {
            string? line;
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Genesis BTC rate", _configuration.GetSection("EmailUserName").Value));
            var bodyBuilder = new BodyBuilder();

            using (var client = new SmtpClient())
            {
                client.Connect(_configuration.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
                client.Authenticate(_configuration.GetSection("EmailUserName").Value, _configuration.GetSection("EmailPassword").Value);

                using (var reader = new StreamReader(_pathToFile))
                {
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        emailMessage.To.Add(new MailboxAddress("To", line));
                        emailMessage.Subject = $"BTC-{currencyName} rate";
                        bodyBuilder.HtmlBody = !String.IsNullOrEmpty(currency) ? $"1 BTC costs: {currency} {currencyName}" : $"Unable to send current BTC in {currencyName} rate";
                        emailMessage.Body = bodyBuilder.ToMessageBody();
                    }

                    await client.SendAsync(emailMessage);
                }

                client.Disconnect(true);
            }
        }
    }
}
