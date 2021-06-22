using System.Collections.Generic;
using Flurl.Http;
using NotionSharp.ApiClient.Lib;
using NotionSharp.ApiClient.Lib.Model;

namespace NotionSharp.ApiClient
{
    /// <summary>
    /// Official integration
    /// </summary>
    public class NotionSession
    {
        public NotionSessionInfo SessionInfo { get; }
        public HttpNotionSession HttpSession { get; }

        public NotionSession(NotionSessionInfo sessionInfo)
        {
            SessionInfo = sessionInfo;

            HttpSession = new HttpNotionSession(client =>
            {
                client.WithHeaders(new Dictionary<string, string>
                {
                    {"Authorization", $"Bearer {sessionInfo.Token}"},
                    {"Notion-Version", Constants.NotionApiVersion}, //Required
                    //{"User-Agent", Constants.UserAgent },
                });
            });
        }
    }
}