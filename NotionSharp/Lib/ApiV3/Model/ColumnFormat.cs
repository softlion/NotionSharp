
using System.Text.Json.Serialization;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class ColumnFormat
    {
        [JsonPropertyName("column_ratio")]
        public float Ratio { get; set; }
    }
}