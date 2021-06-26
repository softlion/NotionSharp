namespace NotionSharp.ApiClient
{
    public class SortOptions
    {
        public SortDirection Direction { get; set; } //"ascending", "descending"
        public SortTimestamp Timestamp { get; set; } //"last_edited_time";

        public static readonly SortOptions? Default = null;
        public static readonly SortOptions LastEditedTimeAscending = new SortOptions { Direction = SortDirection.Ascending, Timestamp = SortTimestamp.LastEditedTime };
        public static readonly SortOptions LastEditedTimeDescending = new SortOptions { Direction = SortDirection.Descending, Timestamp = SortTimestamp.LastEditedTime };
    }
}