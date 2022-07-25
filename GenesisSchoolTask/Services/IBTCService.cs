using GenesisSchoolTask.Models;

namespace GenesisSchoolTask.Services
{
    public interface IBTCService
    {
        Task<RateDto?> GetRate(string connection);
        Task<bool> SignEmailUp(string pathToFile, string email);
        void SendEmails(string pathToFile, string connection, string currency);
    }
}
