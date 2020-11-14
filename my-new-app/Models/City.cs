using Newtonsoft.Json;

namespace HR.Models
{
    public class City
    {
        public string Name { get; set; }
        [JsonProperty("english_name")]
        public string EnglishName { get; set; }
    }
}