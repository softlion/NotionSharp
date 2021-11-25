using System;

namespace DemoNotionBlog.Libs.Services
{
    public class NotionOptions
    {
        public string? Key { get; set; }
        public Guid BrowserId { get; set; }
        public Guid UserId { get; set; }
        public string? CmsPageTitle { get; set; }
    }
}
