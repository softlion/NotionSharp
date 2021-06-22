using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;

namespace NotionSharp.ApiClient
{
    public static class NotionSessionExtensions
    {
        /// <summary>
        /// Searches all pages and child pages that are shared with the integration. The results may include databases.
        /// The query parameter matches against the page titles. If the query parameter is not provided, the response will contain all pages (and child pages) in the results.
        /// The filter parameter can be used to query specifically for only pages or only databases.
        /// </summary>
        /// <remarks>
        /// pageSize max is 100. Default is 10.
        /// Search indexing is not immediate.
        /// You should use HasMore+NextCursor to get more paged results with the same options.
        /// </remarks>
        /// <returns></returns>
        public static async Task<SearchResult?> SearchPaged(this NotionSession session,
            string? query = null, SortOptions? sortOptions = null, FilterOptions? filterOptions = null, PagingOptions? pagingOptions = null,
            CancellationToken cancel = default)
        {
            var searchRequest = new SearchRequest
            {
                Query = query, Sort = sortOptions, Filter = filterOptions, StartCursor = pagingOptions?.StartCursor, PageSize = pagingOptions?.PageSize ?? Constants.DefaultPageSize
            };
            
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "search");
            return await request.PostJsonAsync(searchRequest, cancel).ReceiveJson<SearchResult>().ConfigureAwait(false);
        }
        
        /// <summary>
        /// Searches all pages and child pages that are shared with the integration. The results may include databases.
        /// The query parameter matches against the page titles. If the query parameter is not provided, the response will contain all pages (and child pages) in the results.
        /// The filter parameter can be used to query specifically for only pages or only databases.
        /// </summary>
        /// <remarks>
        /// pageSize max is 100. Default is 10.
        /// Search indexing is not immediate.
        /// Pages are fetched automatically when needed.
        /// </remarks>
        /// <example>
        /// await foreach(var item in SearchAsync().WithCancellation(cancel).ConfigureAwait(false))
        ///    DoSomething(item);
        /// </example>
        public static async IAsyncEnumerable<JsonElement> Search(this NotionSession session,
            string? query = null, SortOptions? sortOptions = null, FilterOptions? filterOptions = null, int pageSize = Constants.DefaultPageSize,
            [EnumeratorCancellation] CancellationToken cancel = default)
        {
            var searchRequest = new SearchRequest { Query = query, Sort = sortOptions, Filter = filterOptions, PageSize = pageSize };
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "search");
            
            while(true)
            {
                var result = await request.PostJsonAsync(searchRequest, cancel).ReceiveJson<SearchResult>().ConfigureAwait(false);
                if(result?.Results == null)
                    yield break;
                foreach (var item in result.Results)
                    yield return item;
                if(!result.HasMore)
                    yield break;
                
                searchRequest.StartCursor = result.NextCursor;
            }
        }

        public static async IAsyncEnumerable<User> GetUsers(this NotionSession session, int pageSize = Constants.DefaultPageSize, [EnumeratorCancellation] CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "users")
                .SetQueryParam("page_size", pageSize);
            
            while(true)
            {
                var result = await request.GetJsonAsync<PaginationResult<User>>(cancel).ConfigureAwait(false);
                if(result?.Results == null)
                    yield break;
                foreach (var item in result.Results)
                    yield return item;
                if(!result.HasMore)
                    yield break;

                request.SetQueryParam("start_cursor", (string)result.NextCursor);
            }
        }

        /// <summary>
        /// Get a user
        /// </summary>
        public static async Task<User?> GetUser(this NotionSession session, string userId, CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "users/" + userId);
            return await request.GetJsonAsync<User>(cancel).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the properties of a page
        /// TODO
        /// </summary>
        public static async Task<JsonElement?> GetPage(this NotionSession session, string pageId, CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "pages/" + pageId);
            return await request.GetJsonAsync<JsonElement>(cancel).ConfigureAwait(false);
        }

        /// <summary>
        /// Get children of a block
        /// TODO 
        /// </summary>
        public static async IAsyncEnumerable<JsonElement>? GetBlockChildren(this NotionSession session, string blockId, int pageSize = Constants.DefaultPageSize, [EnumeratorCancellation] CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "blocks/" + blockId + "/children")
                .SetQueryParam("page_size", pageSize);
            
            while(true)
            {
                var result = await request.GetJsonAsync<PaginationResult<JsonElement>>(cancel).ConfigureAwait(false);
                if(result?.Results == null)
                    yield break;
                foreach (var item in result.Results)
                    yield return item;
                if(!result.HasMore)
                    yield break;

                request.SetQueryParam("start_cursor", (string)result.NextCursor);
            }
        }

        // public static async Task<GetClientExperimentsResult> GetClientExperiments(this NotionSession session, Guid deviceId, CancellationToken cancel = default)
        // {
        //     var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "getClientExperiments");
        //     return await request.PostJsonAsync(new { deviceId = deviceId.ToString("D") }, cancel).ReceiveJson<GetClientExperimentsResult>();
        // }
        //
        // public static async Task<LoadUserContentResult> LoadUserContent(this NotionSession session, CancellationToken cancel = default)
        // {
        //     var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "loadUserContent");
        //     return await request.PostJsonAsync(new object(), cancel).ReceiveJson<LoadUserContentResult>();
        // }
        //
        //
        // public static async Task<LoadPageChunkResult> LoadPageChunk(this NotionSession session, Guid pageId, int chunkNumber = 0, int limit = 50, CancellationToken cancel = default)
        // {
        //     var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "loadPageChunk");
        //     return await request.PostJsonAsync(new LoadPageChunkRequest
        //     {
        //         ChunkNumber = chunkNumber,
        //         Cursor = new Cursor { Stack = new List<List<CursorStack>> { new List<CursorStack> { new CursorStack { Id = pageId, Index = chunkNumber, Table = "block" } } } },
        //         Limit = limit,
        //         PageId = pageId,
        //         VerticalColumns = false
        //     }, cancel).ReceiveJson<LoadPageChunkResult>();
        // }
        //
        // public static async Task<SyndicationFeed> GetSyndicationFeed(this NotionSession session, int maxBlocks = 20, bool stopBeforeFirstSubHeader = true, CancellationToken cancel = default)
        // {
        //     var userContent = await session.LoadUserContent(cancel);
        //     var space = userContent.RecordMap.Space.First().Value;
        //
        //     //collection_view_page not supported
        //     var pages = space.Pages.Where(pageId => userContent.RecordMap.Block[pageId].Type == "page");
        //
        //     var feed = await session.GetSyndicationFeed(pages, maxBlocks, stopBeforeFirstSubHeader, cancel);
        //     feed.Id = space.Id.ToString("N");
        //     feed.Title = new TextSyndicationContent(space.Name);
        //     feed.Description = new TextSyndicationContent(space.Domain);
        //     return feed;
        // }
        //
        // /// <summary>
        // /// Create a syndication feed from a list of page
        // /// </summary>
        // /// <param name="session">a session</param>
        // /// <param name="pages">the pages</param>
        // /// <param name="maxBlocks">limits the parsing of each page to the 1st 20 blocks. Max value: 100</param>
        // /// <param name="stopBeforeFirstSubHeader">when true, stop parsing a page when a line containing a sub_header is found</param>
        // /// <param name="cancel"></param>
        // /// <returns>A SyndicationFeed containing one SyndicationItem per page</returns>
        // /// <remarks>
        // /// The created feed has no title/description
        // /// </remarks>
        // public static async Task<SyndicationFeed> GetSyndicationFeed(this NotionSession session, IEnumerable<Guid> pages, int maxBlocks = 20, bool stopBeforeFirstSubHeader = true, CancellationToken cancel = default)
        // {
        //     //notion's limitation
        //     if (maxBlocks > 100)
        //         throw new ArgumentOutOfRangeException(nameof(maxBlocks));
        //
        //     var feedItems = new List<SyndicationItem>();
        //     foreach (var pageId in pages)
        //     {
        //         //get blocks and extract an html content
        //         var chunks = await session.LoadPageChunk(pageId, 0, maxBlocks, cancel);
        //         var pageBlock = chunks.RecordMap.Block[pageId];
        //
        //         //collection_view_page not supported
        //         if (pageBlock.Permissions?.Any(p => p.Role == Permission.RoleReader && p.Type == Permission.TypePublic) == true
        //             && pageBlock.Type == "page") 
        //         {
        //             //var content = chunks.RecordMap.GetHtmlAbstract(pageId);
        //             var content = chunks.RecordMap.GetHtml(pageId, throwIfBlockMissing: false, stopBeforeFirstSubHeader: stopBeforeFirstSubHeader, throwIfCantDecodeTextData: false);
        //             var pageUri = NotionUtils.GetPageUri(pageId, pageBlock.Title);
        //
        //             var item = new SyndicationItem(pageBlock.Title, content, pageUri)
        //             {
        //                 Id = pageId.ToString("N"),
        //                 BaseUri = pageUri,
        //                 Summary = new TextSyndicationContent(content),
        //                 PublishDate = pageBlock.CreatedTime.EpochToDateTimeOffset(),
        //                 LastUpdatedTime = pageBlock.LastEditedTime.EpochToDateTimeOffset(),
        //             };
        //
        //             if (!String.IsNullOrWhiteSpace(pageBlock.Format?.PageIcon))
        //             {
        //                 if(Uri.TryCreate(pageBlock.Format.PageIcon, UriKind.Absolute, out _))
        //                     item.AttributeExtensions.Add(new XmlQualifiedName("iconUrl"), pageBlock.Format.PageIcon);
        //                 else
        //                     item.AttributeExtensions.Add(new XmlQualifiedName("iconString"), pageBlock.Format.PageIcon);
        //             }
        //
        //             feedItems.Add(item);
        //         }
        //     }
        //
        //     var feed = new SyndicationFeed(feedItems)
        //     {
        //         LastUpdatedTime = feedItems.DefaultIfEmpty().Max(item => item?.LastUpdatedTime ?? DateTimeOffset.MinValue),
        //         //Copyright = 
        //     };
        //
        //     return feed;
        // }
    }
}
