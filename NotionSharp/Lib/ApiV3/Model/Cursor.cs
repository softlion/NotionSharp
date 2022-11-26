using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Cursor
    {
        [JsonPropertyName("stack")]
        public List<List<CursorStack>> Stack { get; set; }
    }
}
