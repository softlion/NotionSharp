using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using NotionSharp.Lib.ApiV3.Results;

namespace NotionSharp
{
    public static class NotionSessionExtensions
    {
        public const string BaseUrl = "https://www.notion.so/";
        public const string ApiBaseUrl = BaseUrl + "api/v3/";
        public const string ClientVersion = "22.3.17";

        public static async Task<GetUserTasksResult> GetUserTasks(this NotionSession session, object data, CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(ApiBaseUrl + "getUserTasks");
            return await request.PostJsonAsync(data, cancel).ReceiveJson<GetUserTasksResult>();
        }

        public static async Task<GetClientExperimentsResult> GetClientExperiments(this NotionSession session, Guid deviceId, CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(ApiBaseUrl + "getClientExperiments");
            return await request.PostJsonAsync(new { deviceId = deviceId.ToString("D") }, cancel).ReceiveJson<GetClientExperimentsResult>();
        }

        public static async Task<LoadUserContentResult> LoadUserContent(this NotionSession session, CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(ApiBaseUrl + "loadUserContent");
            return await request.PostJsonAsync(new object(), cancel).ReceiveJson<LoadUserContentResult>();
        }

        /// <summary>
        /// Extract the block/page ID from a Notion.so URL
        /// if it's a bare page URL, it will be the ID of the page.
        /// If there's a hash with a block ID in it (from clicking "Copy Link") on a block in a page), it will instead be the ID of that block.
        /// If it's already in ID format, it will be passed right through.
        /// </summary>
        /// <example>
        /// https://www.notion.so/vapolia/Secret-feature-Xamarin-Forms-control-s-auto-registration-1fd6f1b0d98d4aabb2defa0eb14961fa
        /// https://www.notion.so/vapolia/Secret-feature-Xamarin-Forms-control-s-auto-registration-1fd6f1b0d98d4aabb2defa0eb14961fa#1678f28a66d7486b93c4c98a1caf7967
        /// </example>
        public static Guid? ExtractId(string urlOrId)
        {
            if (urlOrId.StartsWith(BaseUrl))
                urlOrId = new Uri(urlOrId).GetLeftPart(UriPartial.Path).Split('-').Last().Split('#').Last();

            if (Guid.TryParse(urlOrId, out var guid))
                return guid;

            return null;
        }
    }
}
