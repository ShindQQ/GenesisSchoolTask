using GenesisSchoolTask.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GenesisSchoolTask.Controllers
{
    /// <summary>
    /// Controller to work with API
    /// </summary>
    [ApiController]
    [Route("api/rate")]
    public class BTC_UAHRateContorller : ControllerBase
    {
        private readonly string connection = "https://www.kucoin.com/_api/currency/v2/prices?base=UAH";
        private readonly string pathToFile = "emails.txt";
        private readonly IBTCService _BTCService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bTCService">Interface which we are implementing using dependency injection</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BTC_UAHRateContorller(IBTCService bTCService)
        {
            _BTCService = bTCService ?? throw new ArgumentNullException(nameof(bTCService));
        }

        /// <summary>
        /// Getting rate of selected cryptocurrency
        /// </summary>
        /// <returns>An ActionResult of string</returns>
        /// <response code="200">Returns the requested currency</response>
        /// <response code="404">Returns status code 404 if there was no rate for such request</response>
        [HttpGet("rate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<string>> GetRate()
        {
            using var client = new HttpClient();

            var content = await _BTCService.GetRate(connection);

            if (content == null)
            {
                return NotFound();
            }

            content.data.TryGetValue("BTC", out string? currency);

            return Ok(currency);
        }

        /// <summary>
        /// Signing email up on notification
        /// </summary>
        /// <param name="email">Receiving email to sign it up</param>
        /// <returns>An ActionResult</returns>
        /// <response code="200">Nothing to return, status code 200</response>
        /// <response code="400">Returns status code 400 if email is null or it isn`t valid or when it was enable to sign it up</response>
        /// <response code="404">Returns status code 404 if there was no file to add in</response>
        [HttpPost("{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Sending email on signed emails
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Nothing to return, status code 200</response>
        /// <response code="404">Returns status code 404 if there was no data in request or if it was unable to find currency</response>
        [HttpPost("sendRate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> SendEmails() // We could use HangFire
        {
            var content = await _BTCService.GetRate(connection);

            if (content == null)
            {
                return NotFound();
            }

            content.data.TryGetValue("BTC", out string? currency);

            if (currency == null)
            {
                return NotFound();
            }

            _BTCService.SendEmails(pathToFile, connection, currency);

            return Ok();
        }
    }
}
