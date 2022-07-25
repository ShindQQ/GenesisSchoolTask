namespace GenesisSchoolTask.Models
{
    public class RateDto
    {
        public bool success { get; set; }
        public string code { get; set; } = string.Empty;
        public string msg { get; set; } = string.Empty;
        public bool retry { get; set; }

        public Dictionary<string, string> data { get; set; }
    }
}

