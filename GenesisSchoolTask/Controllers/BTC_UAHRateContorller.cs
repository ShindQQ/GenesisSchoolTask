using GenesisSchoolTask.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GenesisSchoolTask.Controllers
{
    [ApiController]
    [Route("api/rate")]
    public class BTC_UAHRateContorller : ControllerBase
    {
        private readonly string connection = "https://www.kucoin.com/_api/currency/v2/prices?base=UAH";
        private readonly string pathToFile = "emails.txt";
        private readonly IBTCService _BTCService;

        public BTC_UAHRateContorller(IBTCService bTCService)
        {
            _BTCService = bTCService ?? throw new ArgumentNullException(nameof(bTCService));
        }

        [HttpGet("rate")]
        public async Task<ActionResult<string>> GetRate()
        {
            using var client = new HttpClient();

            var content = await _BTCService.GetRate(connection);

            if (content == null)
            {
                return NotFound();
            }

            content.data.TryGetValue("BTC", out string? UAH);

            return Ok(UAH);
        }

        [HttpPost("{email}")]
        public async Task<ActionResult> SignEmailUp([Required] [EmailAddress] string? email)
        {
            if (email == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!System.IO.File.Exists(pathToFile))
            {
                return NotFound();
            }

            if (! await _BTCService.SignEmailUp(pathToFile, email))
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost("sendRate")]
        public async Task<ActionResult> SendEmails() // We could use HangFire
        {
            var content = await _BTCService.GetRate(connection);

            if (content == null)
            {
                return NotFound();
            }

            content.data.TryGetValue("BTC", out string? currency);

            _BTCService.SendEmails(pathToFile, connection, currency);

            return Ok();
        }
    }
}
