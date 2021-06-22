using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient
{
    public enum SortTimestamp
    {
        [JsonPropertyName("last_edited_time")]
        LastEditedTime
    }
}