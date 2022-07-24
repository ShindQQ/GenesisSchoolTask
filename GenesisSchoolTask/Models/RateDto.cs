namespace GenesisSchoolTask.Models
{
    public class RateDto
    {
        public bool success { get; set; }
        public string code { get; set; }
        public string msg { get; set; }
        public bool retry { get; set; }

        public Dictionary<string, string> data { get; set; }
    }
}

