using System;

namespace NotionSharp
{
    /// <summary>
    /// From local Cookie, sent with every request
    /// </summary>
    public struct NotionSessionInfo
    {
        public Guid NotionBrowserId { get; set; }
        public string TokenV2 { get; set; }
        public Guid NotionUserId { get; set; }
        public string NotionUserAgent { get; set; }
    }
}