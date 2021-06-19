using System.Collections.Generic;
using Flurl.Http;

namespace NotionSharp
{
    /// <summary>
    /// Official integration
    /// </summary>
    public class NotionSession2
    {
        public NotionSessionInfo2? SessionInfo2 { get; }
        public HttpCookieSession HttpSession { get; }

        public NotionSession2(NotionSessionInfo2 sessionInfo)
        {
            SessionInfo2 = sessionInfo;

            HttpSession = new HttpCookieSession(client =>
            {
                client.WithHeaders(new Dictionary<string, string>
                {
                    {"Authorization", $"Bearer {sessionInfo.Token}"},
                    {"notion-client-version", Constants.ClientVersion},
                    {"User-Agent", Constants.UserAgent },
                });
            });
        }
    }
}