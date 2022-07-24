using GenesisSchoolTask.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;

namespace GenesisSchoolTask.Controllers
{
    [ApiController]
    [Route("api/rate")]
    public class BTC_UAHRateContorller : ControllerBase
    {
        private readonly string pathToFile = "emails.txt";

        [HttpGet]
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

        [HttpPost]
        public async Task<ActionResult> SendEmails()
        {
            var from = new MailAddress("rategenesis@gmail.com", "GenesisSchool");
            string? line;
            SmtpClient smtpClient =  new SmtpClient("smtp.office365.com", 25)
            {
                Credentials = new NetworkCredential("rategenesis@gmail.com", "notme141421"),
                EnableSsl = true
            };

            using (var reader = new StreamReader(pathToFile))
            {
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var to = new MailAddress(line);
                    MailMessage m = new MailMessage(from, to);
                    var msg = new MailMessage(from, to)
                    {
                        Subject = "BTC-UAH rate",
                        Body = GetRate().Result.Value?.ToString(),
                        IsBodyHtml = false
                    };
                    smtpClient.Send(m);
                }
            }

            return Ok();
        }
    }
}
