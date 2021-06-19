using System;
using System.Collections.Generic;
using Flurl.Http;

namespace NotionSharp
{
    public class NotionSession
    {
        public NotionSessionInfo? SessionInfo { get; }
        public HttpCookieSession HttpSession { get; }

        /// <summary>
        /// Unofficial integration
        /// </summary>
        public NotionSession(NotionSessionInfo sessionInfo)
        {
            SessionInfo = sessionInfo;

            HttpSession = new HttpCookieSession(client =>
            {
                client.WithHeaders(new Dictionary<string, string>
                {
                    { "x-notion-active-user-header", sessionInfo.NotionUserId.ToString("D") },
                    { "notion-client-version", Constants.ClientVersion },
                    { "User-Agent", Constants.UserAgent },
                });
            })
            .WithCookies("https://www.notion.so", new Dictionary<string, string>
            {
                { "notion_browser_id", sessionInfo.NotionBrowserId.ToString("D") },
                { "token_v2", sessionInfo.TokenV2},
                { "notion_user_id", sessionInfo.NotionUserId.ToString("D")},
                { "notion_users", Uri.EscapeDataString($"[\"{sessionInfo.NotionUserId:D}\"]")},
            });
        }
    }
}
