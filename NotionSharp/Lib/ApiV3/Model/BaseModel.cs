using System;
using Newtonsoft.Json.Linq;
using NotionSharp.Lib.ApiV3.Enums;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class BaseModel
    {
        public Role Role { get; set; }
        public JObject Value { get; set; }

        public Guid Id => Guid.Parse((string)Value["id"]);
        public int Version => int.Parse((string)Value["version"]);
    }
}
