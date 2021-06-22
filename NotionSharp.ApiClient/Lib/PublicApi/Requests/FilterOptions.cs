namespace NotionSharp.ApiClient
{
    public class FilterOptions
    {
        /// <summary>
        /// Value: Possible Properties
        /// object: page, database
        /// </summary>
        public string Value { get; set; } = "object";
        public string Property { get; set; } = "page";
    }
}