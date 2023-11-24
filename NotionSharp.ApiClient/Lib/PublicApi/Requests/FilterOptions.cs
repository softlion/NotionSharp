namespace NotionSharp.ApiClient;

public class FilterOptions
{
    /// <summary>
    /// "object"
    /// </summary>
    public string Property { get; set; }
    /// <summary>
    /// object: "page", "database"
    /// </summary>
    public string Value { get; set; }

    public static readonly FilterOptions ObjectPage = new() {Property = "object", Value = "page"};
    public static readonly FilterOptions ObjectDatabase = new() {Property = "object", Value = "database"};
}