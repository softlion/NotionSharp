using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotionSharp.Lib.ApiV3.Model;

public class BaseModel
{
    public JsonElement TheValue => Values["value"];
        
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Values { get; set; }

    public Guid Id => Guid.Parse(TheValue.GetProperty("id").GetString());
    public int Version => TheValue.GetProperty("version").GetInt32();
}