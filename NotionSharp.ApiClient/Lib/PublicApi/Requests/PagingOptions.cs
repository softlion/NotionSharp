using System;

namespace NotionSharp.ApiClient
{
    public struct PagingOptions
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
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException(nameof(PageSize));
                pageSize = value;
            }
        }
    }
}