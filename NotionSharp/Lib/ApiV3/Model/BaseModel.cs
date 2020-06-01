using System;
using Newtonsoft.Json.Linq;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class BaseModel
    {
        public JObject Value { get; set; }

        public Guid Id => Guid.Parse((string)Value["id"]);
        public int Version => int.Parse((string)Value["version"]);
    }
}
