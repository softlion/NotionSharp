using System;
using System.Collections.Generic;
using Flurl.Http;
using NotionSharp.Lib;

namespace NotionSharp
{
    public class NotionSession
    {
        public NotionSessionInfo SessionInfo { get; }
        public HttpSession HttpSession { get; }

        public NotionSession(NotionSessionInfo sessionInfo)
        {
            SessionInfo = sessionInfo;

            HttpSession = new HttpSession(client =>
            {
                client.WithCookies(new Dictionary<string, string>
                {
                    { "notion_browser_id", sessionInfo.NotionBrowserId.ToString("D") },
                    { "token_v2", sessionInfo.TokenV2},
                    { "notion_user_id", sessionInfo.NotionUserId.ToString("D")},
                    { "notion_users", Uri.EscapeDataString($"[\"{sessionInfo.NotionUserId:D}\"]")},
                });

                client.WithHeaders(new Dictionary<string, string>
                {
                    { "x-notion-active-user-header", sessionInfo.NotionUserId.ToString("D") },
                    { "notion-client-version", Constants.ClientVersion },
                    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.122 Safari/537.36" },
                });
            });
        }
    }
}