using GenesisSchoolTask.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.ComponentModel.DataAnnotations;

namespace GenesisSchoolTask.Controllers
{
    [ApiController]
    [Route("api/rate")]
    public class BTC_UAHRateContorller : ControllerBase
    {
        private readonly string pathToFile = "emails.txt";

        [HttpGet("rate")]
        public async Task<ActionResult<string>> GetRate()
        {
            using var client = new HttpClient();

            var content = await client.GetFromJsonAsync<RateDto>("https://www.kucoin.com/_api/currency/v2/prices?base=UAH");

            if (content == null)
            {
                return NotFound();
            }

            if (!content.data.TryGetValue("BTC", out string? UAH))
            {
                return NotFound();
            }
            
            return Ok(UAH);
        }

        [HttpPost("{email}")]
        public async Task<ActionResult> SignEmailUp([Required] [EmailAddress] string? email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
           
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
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost("sendRate")]
        public async Task<ActionResult> SendEmails() // We could use HangFire
        {
            string? line;

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("GenesisSchool", "rategenesis@gmail.com"));
            var bodyBuilder = new BodyBuilder();

            string UAH = await GetRateInUAH();

            using (var client = new SmtpClient())
            {
                var secureSocketOptions = SecureSocketOptions.None;
                client.Connect("localhost", 25, secureSocketOptions);

                using (var reader = new StreamReader(pathToFile))
                {
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        emailMessage.To.Add(new MailboxAddress("To", line));
                        emailMessage.Subject = "BTC-UAH rate";
                        bodyBuilder.HtmlBody = String.IsNullOrEmpty(UAH) ? UAH : "Unable to send current BTC in UAH rate";
                        emailMessage.Body = bodyBuilder.ToMessageBody();
                        await client.SendAsync(emailMessage);
                    }
                }

                client.Disconnect(true);
            }

            return Ok();
        }

        private async Task<string> GetRateInUAH()
        {
            using var client = new HttpClient();

            var content = await client.GetFromJsonAsync<RateDto>("https://www.kucoin.com/_api/currency/v2/prices?base=UAH");

            if (content == null)
            {
                return string.Empty;
            }

            if (!content.data.TryGetValue("BTC", out string? UAH))
            {
                return string.Empty;
            }

            return UAH;
        }
    }
}
