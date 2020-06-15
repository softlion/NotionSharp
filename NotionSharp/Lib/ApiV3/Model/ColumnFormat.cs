using Newtonsoft.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class ColumnFormat
    {
        [JsonProperty("column_ratio")]
        public float Ratio { get; set; }
    }
}