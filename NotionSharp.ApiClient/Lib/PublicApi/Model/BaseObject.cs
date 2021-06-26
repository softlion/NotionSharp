using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient
{
    public abstract class BaseObject
    {
        [JsonIgnore]
        public string? JsonOriginal { get; set; }

        public virtual string Object { get; set; } = null!; 
    }
}
