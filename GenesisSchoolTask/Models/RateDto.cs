namespace GenesisSchoolTask.Models
{
    /// <summary>
    /// Class to work with dictionary of cryptocurrency and it`s rate 
    /// </summary>
    public class RateDto
    {
        /// <summary>
        /// Dictionary of cryptocurrency and it`s rate
        /// </summary>
        public Dictionary<string, string> data { get; set; } = new Dictionary<string, string>();
    }
}

