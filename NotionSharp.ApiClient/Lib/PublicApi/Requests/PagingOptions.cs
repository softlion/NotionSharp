using System;

namespace NotionSharp.ApiClient;

public record PagingOptions
{
    private int pageSize;
        
    public string? StartCursor { get; set; }

    /// <summary>
    /// max: 100
    /// </summary>
    public int PageSize
    {
        get => pageSize;
        set
        {
            if (value is < 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(PageSize));
            pageSize = value;
        }
    }
}