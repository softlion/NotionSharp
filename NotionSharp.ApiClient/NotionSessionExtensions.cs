using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using NotionSharp.ApiClient.Lib;
using NotionSharp.ApiClient.Lib.HtmlRendering;

namespace NotionSharp.ApiClient
{
    public static class NotionSessionExtensions
    {
        /// <summary>
        /// Use Search instead. I'm keeping this method around to see if it may have some use case. Comment on github's issues plz.
        /// 
        /// Searches all pages and child pages that are shared with the integration. The results may include databases.
        /// The query parameter matches against the page titles. If the query parameter is not provided, the response will contain all pages (and child pages) in the results.
        /// The filter parameter can be used to query specifically for only pages or only databases.
        /// </summary>
        /// <remarks>
        /// The query parameter matches against the page titles.
        /// pageSize max is 100. Default is 10.
        /// Search indexing is not immediate.
        /// You should use HasMore+NextCursor to get more paged results with the same options.
        /// </remarks>
        [Obsolete("Use Search() instead of SearchPaged()")]
        public static async Task<PaginationResult<Page>?> SearchPaged(this NotionSession session,
            string? query = null, SortOptions? sortOptions = null, FilterOptions? filterOptions = null, PagingOptions? pagingOptions = null,
            CancellationToken cancel = default)
        {
            var searchRequest = new SearchRequest
            {
                Query = query, Sort = sortOptions, Filter = filterOptions, StartCursor = pagingOptions?.StartCursor, PageSize = pagingOptions?.PageSize ?? Constants.DefaultPageSize
            };
            
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "search");
            return await request.PostJsonAsync(searchRequest, cancel).ReceiveJson<PaginationResult<Page>>().ConfigureAwait(false);
        }
        
        /// <summary>
        /// Searches all pages and child pages that are shared with the integration. The results may include databases.
        /// The query parameter matches against the page titles. If the query parameter is not provided, the response will contain all pages (and child pages) in the results.
        /// The filter parameter can be used to query specifically for only pages or only databases.
        /// </summary>
        /// <remarks>
        /// The query parameter matches against the page titles.
        /// pageSize max is 100. Default is 10.
        /// Search indexing is not immediate.
        /// Pages are fetched automatically when needed.
        /// </remarks>
        /// <example>
        /// await foreach(var item in SearchAsync().WithCancellation(cancel).ConfigureAwait(false))
        ///    DoSomething(item);
        /// </example>
        public static async IAsyncEnumerable<Page> Search(this NotionSession session,
            string? query = null, SortOptions? sortOptions = null, FilterOptions? filterOptions = null, int pageSize = Constants.DefaultPageSize,
            [EnumeratorCancellation] CancellationToken cancel = default)
        {
            var searchRequest = new SearchRequest { Query = query, Sort = sortOptions, Filter = filterOptions, PageSize = pageSize };
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "search");
            
            while(true)
            {
                var result = await request.PostJsonAsync(searchRequest, cancel).ReceiveJson<PaginationResult<Page>>().ConfigureAwait(false);
                if(result?.Results == null)
                    yield break;
                foreach (var item in result.Results)
                    yield return item;
                if(!result.HasMore || result.NextCursor == null)
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
                if(!result.HasMore || result.NextCursor == null)
                    yield break;

                request.SetQueryParam("start_cursor", result.NextCursor);
            }
        }

        /// <summary>
        /// Get a user
        /// </summary>
        public static async Task<User?> GetUser(this NotionSession session, string userId, CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "users/" + userId);
            return await request.GetJson<User>(cancel).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the properties of a page
        /// </summary>
        /// <remarks>
        /// (from API doc)
        /// Each page property is computed with a limit of 25 page references.
        /// Therefore relation property values feature a maximum of 25 relations,
        /// rollup property values are calculated based on a maximum of 25 relations,
        /// and rich text property values feature a maximum of 25 page mentions.
        /// </remarks>
        public static async Task<Page?> GetPage(this NotionSession session, string pageId, CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "pages/" + pageId);
            return await request.GetJson<Page>(cancel).ConfigureAwait(false);
        }


        /// <summary>
        /// Get children of a block (ie: its content)
        /// A block is any object having an Id.
        /// </summary>
        public static async IAsyncEnumerable<Block> GetBlockChildren(this NotionSession session, string blockId, int pageSize = Constants.DefaultPageSize, [EnumeratorCancellation] CancellationToken cancel = default)
        {
            var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "blocks/" + blockId + "/children")
                .SetQueryParam("page_size", pageSize);

            while(true)
            {
                var result = await request.GetJsonAsync<PaginationResult<Block>>(cancel).ConfigureAwait(false);
                if(result?.Results == null)
                    yield break;
                foreach (var item in result.Results)
                    yield return item;
                if(!result.HasMore || result.NextCursor == null)
                    yield break;

                request.SetQueryParam("start_cursor", result.NextCursor);
            }
        }

        public static async Task<SyndicationFeed> GetSyndicationFeed(this NotionSession session, int maxBlocks = 20, bool stopBeforeFirstSubHeader = true, CancellationToken cancel = default)
        {
            var pages = await session.Search(filterOptions: FilterOptions.ObjectPage, cancel: cancel)
                .Where(pageProp => pageProp.Parent.Type == Page.PageParent.TypeWorkspace) //Only top level pages
                .Take(maxBlocks)
                .ToListAsync(cancel).ConfigureAwait(false);

            if (pages.Count == 0)
                return new SyndicationFeed { LastUpdatedTime = DateTimeOffset.Now };
                    
            var feed = await session.GetSyndicationFeed(pages, maxBlocks, stopBeforeFirstSubHeader, cancel).ConfigureAwait(false);
            //"workspace" data not available in API
            //feed.Id = space.Id.ToString("N");
            //feed.Title = new TextSyndicationContent(space.Name);
            //feed.Description = new TextSyndicationContent(space.Domain);
            feed.Id = pages[0].Id;
            feed.Title = new TextSyndicationContent(pages[0].Title?.Title[0]?.Text?.Content ?? "");
            return feed;
        }

        /// <summary>
        /// Create a syndication feed from a list of page
        /// </summary>
        /// <param name="session">a session</param>
        /// <param name="pages">the pages</param>
        /// <param name="maxBlocks">limits the parsing of each page to the 1st 20 blocks. Max value: 100</param>
        /// <param name="stopBeforeFirstSubHeader">when true, stop parsing a page when a line containing a sub_header is found</param>
        /// <param name="cancel"></param>
        /// <returns>A SyndicationFeed containing one SyndicationItem per page</returns>
        /// <remarks>
        /// The created feed has no title/description
        /// </remarks>
        public static async Task<SyndicationFeed> GetSyndicationFeed(this NotionSession session, IEnumerable<Page> pages, int maxBlocks = 20, bool stopBeforeFirstSubHeader = true, CancellationToken cancel = default)
        {
            var feedItems = new List<SyndicationItem>();
            var htmlRenderer = new HtmlRenderer();
            
            foreach (var page in pages)
            {
                //get blocks and extract an html content
                var blocks = await session.GetBlockChildren(page.Id, cancel: cancel)
                    .Take(maxBlocks)
                    .ToListAsync(cancel).ConfigureAwait(false);
        
                var content = htmlRenderer.GetHtml(blocks, stopBeforeFirstSubHeader);
                var title = page.Title?.Title[0]?.PlainText;
                var pageUri = NotionUtils.GetPageUri(page.Id, title);
    
                var item = new SyndicationItem(title, content, pageUri)
                {
                    Id = page.Id,
                    BaseUri = pageUri,
                    Summary = new TextSyndicationContent(content),
                    PublishDate = page.CreatedTime,
                    LastUpdatedTime = page.LastEditedTime,
                };

                // Property not yet available in API
                // if (!String.IsNullOrWhiteSpace(page.Format?.PageIcon))
                // {
                //     if(Uri.TryCreate(page.Format.PageIcon, UriKind.Absolute, out _))
                //         item.AttributeExtensions.Add(new XmlQualifiedName("iconUrl"), pageBlock.Format.PageIcon);
                //     else
                //         item.AttributeExtensions.Add(new XmlQualifiedName("iconString"), pageBlock.Format.PageIcon);
                // }
    
                feedItems.Add(item);
            }
        
            var feed = new SyndicationFeed(feedItems)
            {
                LastUpdatedTime = feedItems.DefaultIfEmpty().Max(item => item?.LastUpdatedTime ?? DateTimeOffset.MinValue),
            };
        
            return feed;
        }
    }
}
