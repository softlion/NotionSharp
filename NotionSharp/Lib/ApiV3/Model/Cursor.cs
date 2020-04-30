using System.Collections.Generic;
using Newtonsoft.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Cursor
    {
        [JsonProperty("stack")]
        public List<List<CursorStack>> Stack { get; set; }
    }
}
