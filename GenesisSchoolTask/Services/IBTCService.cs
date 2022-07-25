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
        /// <param name="connection">String with params of btc and currency to receive</param>
        /// <returns>RateDto which has dictionary with cryptocurrency infos</returns>
        Task<RateDto?> GetRate(string connection);

        /// <summary>
        /// Signing email on notification of rate
        /// </summary>
        /// <param name="pathToFile">Path to file with users signed on notification</param>
        /// <param name="email">Email which we will add on notification</param>
        /// <returns>True if everything is ok, and false if we had such email if our file</returns>
        Task<bool> SignEmailUp(string pathToFile, string email);

        /// <summary>
        /// Sending emails to all users
        /// </summary>
        /// <param name="pathToFile">Path to file with users signed on notification</param>
        /// <param name="connection">API from which we are receiving cryptocurrency rate</param>
        /// <param name="currency">Current rate of needed currency</param>
        void SendEmails(string pathToFile, string connection, string currency);
    }
}
