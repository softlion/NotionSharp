using System.Text.Json;

namespace NotionSharp.ApiClient
{
    /// <summary>
    /// required for system.text.json
    /// </summary>
    public class SearchResult : PaginationResult<JsonElement>
    {
        //Object = "list"
    }
}