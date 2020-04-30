using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using NotionSharp.Lib;
using NotionSharp.Lib.ApiV3.Model;
using NotionSharp.Lib.ApiV3.Requests;
using NotionSharp.Lib.ApiV3.Results;

namespace NotionSharp
{
    public static class NotionSessionExtensions
    {
        public static async Task<GetUserTasksResult> GetUserTasks(this NotionSession session, object data, CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "getUserTasks");
            return await request.PostJsonAsync(data, cancel).ReceiveJson<GetUserTasksResult>();
        }

        public static async Task<GetClientExperimentsResult> GetClientExperiments(this NotionSession session, Guid deviceId, CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "getClientExperiments");
            return await request.PostJsonAsync(new { deviceId = deviceId.ToString("D") }, cancel).ReceiveJson<GetClientExperimentsResult>();
        }

        public static async Task<LoadUserContentResult> LoadUserContent(this NotionSession session, CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "loadUserContent");
            return await request.PostJsonAsync(new object(), cancel).ReceiveJson<LoadUserContentResult>();
        }


        public static async Task<LoadPageChunkResult> LoadPageChunk(this NotionSession session, Guid pageId, int chunkNumber = 0, int limit = 50, CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "loadPageChunk");
            return await request.PostJsonAsync(new LoadPageChunkRequest
            {
                ChunkNumber = chunkNumber,
                Cursor = new Cursor { Stack = new List<List<CursorStack>> { new List<CursorStack> { new CursorStack { Id = pageId, Index = chunkNumber, Table = "block" } } } },
                Limit = limit,
                PageId = pageId,
                VerticalColumns = false
            }, cancel).ReceiveJson<LoadPageChunkResult>();
        }

        public static async Task<SyndicationFeed> GetSyndicationFeed(this NotionSession session, CancellationToken cancel = default)
        {
            var userContent = await session.LoadUserContent(cancel);

            var space = userContent.RecordMap.Space.First().Value;

            var feedItems = new List<SyndicationItem>();
            foreach (var pageId in space.Pages)
            {
                //Skip collections
                if(userContent.RecordMap.Block[pageId].Type != "page") //collection_view_page not supported
                    continue;

                //get blocks and extract an html content
                var chunks = await session.LoadPageChunk(pageId, 0, 20, cancel);
                var pageBlock = chunks.RecordMap.Block[pageId]; //Get the latest version of the page block

                if (pageBlock.Alive)
                {
                    var content = chunks.RecordMap.GetHtmlAbstract(pageId);

                    //var pageBlock = userContent.RecordMap.Block[pageId];
                    var pageUri = NotionUtils.GetPageUri(pageId, pageBlock.Title);

                    feedItems.Add(new SyndicationItem(pageBlock.Title, content, pageUri)
                    {
                        PublishDate = pageBlock.CreatedTime.EpochToDateTimeOffset(),
                        LastUpdatedTime = pageBlock.LastEditedTime.EpochToDateTimeOffset(),
                    });
                }
            }

            var feed = new SyndicationFeed(space.Name, space.Domain, null, feedItems)
            {
                //Copyright = 
            };

            return feed;
        }
    }
}
