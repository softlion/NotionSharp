namespace NotionSharp.ApiClient
{
    public struct SortOptions
    {
        public SortDirection Direction { get; set; } //"ascending", "descending"
        public SortTimestamp Timestamp { get; set; } //"last_edited_time";
    }
}