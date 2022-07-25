using GenesisSchoolTask.Models;

namespace GenesisSchoolTask.Services
{
    /// <summary>
    /// Interface for implementing to work with API
    /// </summary>
    public interface IBTCService
    {
        /// <summary>
        /// Receiving rate of btc
        /// </summary>
        /// <returns>RateDto which has dictionary with cryptocurrency infos</returns>
        Task<RateDto?> GetRate();

        /// <summary>
        /// Signing email on notification of rate
        /// </summary>
        /// <param name="email">Email which we will add on notification</param>
        /// <returns>True if everything is ok, and false if we had such email if our file</returns>
        Task<bool> SignEmailUp(string email);

        /// <summary>
        /// Sending emails to all users
        /// </summary>
        /// <param name="currency">Current rate of needed currency</param>
        /// <param name="currencyName">Current name of needed currency</param>
        void SendEmails(string currency, string currencyName);
    }
}
