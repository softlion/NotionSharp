using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotionSharp.Lib.ApiV3.Model;

public class BaseModel
{
    public JsonElement Value => Values["value"];
        
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Values { get; set; }

    public Guid Id => Guid.Parse(Value.GetProperty("id").GetString());
    public int Version => Value.GetProperty("version").GetInt32();
}