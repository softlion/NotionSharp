using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using FluentRest.Http;
using FluentRest.Http.Content;
using NotionSharp.ApiClient.Lib;
using NotionSharp.ApiClient.Lib.HtmlRendering;

namespace NotionSharp.ApiClient;

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
            
        var request = session.HttpSession.CreateRequest($"{Constants.ApiBaseUrl}search");
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
        var request = session.HttpSession.CreateRequest($"{Constants.ApiBaseUrl}search");
        
        while(true)
        {
            var response = await request.AllowAnyHttpStatus().PostJsonAsync(searchRequest, cancel);
            
            if (response.StatusCode is 400)
            {
                var errorResult = await response.GetStringAsync();
                throw new NotionApiException(null, errorResult);
            }
            if (response.StatusCode is not (>= 200 and < 300))
                yield break;

#if DEBUG
            var jsonOriginal = await response.GetStringAsync();
#endif

            var result = await response.GetJsonAsync<PaginationResult<Page>>();

            if(result?.Results == null || cancel.IsCancellationRequested)
                yield break;
            foreach (var item in result.Results)
                yield return item;
            if(!result.HasMore || result.NextCursor == null)
                yield break;
                
            searchRequest = searchRequest with { StartCursor = result.NextCursor };
        }
    }

    public static IAsyncEnumerable<Page> GetTopLevelPages(this NotionSession session, CancellationToken cancel = default)
        => session.Search(filterOptions: FilterOptions.ObjectPage, cancel: cancel)
            .Where(page => page.ParentIsWorkspace());


    public static async IAsyncEnumerable<User> GetUsers(this NotionSession session, int pageSize = Constants.DefaultPageSize, [EnumeratorCancellation] CancellationToken cancel = default)
    {
        var request = session.HttpSession.CreateRequest($"{Constants.ApiBaseUrl}users")
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
        var request = session.HttpSession.CreateRequest($"{Constants.ApiBaseUrl}users/{userId}");
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
        var request = session.HttpSession.CreateRequest($"{Constants.ApiBaseUrl}pages/{pageId}");
        return await request.GetJson<Page>(cancel).ConfigureAwait(false);
    }


    /// <summary>
    /// Get children of a block (ie: its content)
    /// A block is any object having an Id.
    /// </summary>
    public static async IAsyncEnumerable<Block> GetBlockChildren(this NotionSession session, string blockId, int pageSize = Constants.DefaultPageSize, [EnumeratorCancellation] CancellationToken cancel = default)
    {
        var request = session.HttpSession.CreateRequest($"{Constants.ApiBaseUrl}blocks/{blockId}/children")
            .SetQueryParam("page_size", pageSize);

        while(true)
        {
            //var result = await request.GetJsonAsync<PaginationResult<Block>>(cancel).ConfigureAwait(false);
            var response = await request.AllowAnyHttpStatus().SendAsync(HttpMethod.Get, cancellationToken: cancel);
#if DEBUG
            var jsonOriginal = await response.GetStringAsync();
#endif
            var result = await response.GetJsonAsync<PaginationResult<Block>>().ConfigureAwait(false);;
            
            if (result == null || response.StatusCode is 400)
            {
                var errorResult = await response.GetStringAsync();
                throw new NotionApiException(null, errorResult);
            }

            if(result.Results == null)
                yield break;
            foreach (var item in result.Results)
                yield return item;
            if(!result.HasMore || result.NextCursor == null)
                yield break;

            request.SetQueryParam("start_cursor", result.NextCursor);
        }
    }
    
    /// <summary>
    /// Page properties are most useful when interacting with a page that is an entry in a database, represented as a row in the Notion UI.
    /// If a page is not part of a database, then its only available property is its title.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="pageId"></param>
    /// <param name="propertyId"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancel"></param>
    /// <returns></returns>
    /// <exception cref="NotionApiException"></exception>
    public static async IAsyncEnumerable<PropertyItem> GetPageProperty(this NotionSession session, string pageId, string propertyId, int pageSize = Constants.DefaultPageSize, [EnumeratorCancellation] CancellationToken cancel = default)
    {
        if (propertyId == "title")
            throw new NotionApiException(null, "Can't get property 'title', it is always available on the Page itself");
        
        var request = session.HttpSession.CreateRequest($"{Constants.ApiBaseUrl}pages/{pageId}/properties{propertyId}")
            .SetQueryParam("page_size", pageSize);

        while(true)
        {
            //var result = await request.AllowAnyHttpStatus().GetJsonAsync<PaginationResult<PropertyItem>>(cancel).ConfigureAwait(false);
            var response = await request.AllowAnyHttpStatus().SendAsync(HttpMethod.Get, cancellationToken: cancel);
            var result = await response.GetJsonAsync<PaginationResult<PropertyItem>>().ConfigureAwait(false);;

            if (result == null || response.StatusCode is 400)
            {
                var errorResult = await response.GetStringAsync();
                throw new NotionApiException(new(errorResult), $"GetPageProperty(propertyId:{propertyId}) failed");
            }

            if (result.Object == "list")
            {
                foreach (var item in result.Results)
                    yield return item;
                if (!result.HasMore || result.NextCursor == null)
                    yield break;
                request.SetQueryParam("start_cursor", result.NextCursor);
            }
            else if (result.Object == "property_item")
            {
                yield return session.HttpSession.JsonSerializer.Deserialize<PropertyItem>(result.JsonOriginal);
                yield break;
            }
            else
            {
#if DEBUG
                throw new NotionApiException(null, $"Unknown object type {result.Object}");
#endif
                yield break;
            }
        }
    }

    /// <summary>
    /// Fetch the child blocks of this block
    /// </summary>
    /// <param name="session"></param>
    /// <param name="block"></param>
    /// <param name="cancel"></param>
    public static async Task GetChildren(this NotionSession session, Block block, CancellationToken cancel = default)
    {
        block.Children = await session.GetBlockChildren(block.Id, cancel: cancel)
            .Where(childBlock => BlockTypes.SupportedBlocks.Contains(childBlock.Type))
            .ToListAsync(cancel).ConfigureAwait(false);
    }
        
    public static async Task<string> GetHtml(this NotionSession session, Page page, CancellationToken cancel = default)
    {
        var blocks = await session.LoadPageBlocks(page, cancel).ConfigureAwait(false);
        if (blocks.Count == 0)
            return string.Empty;
        
        return new HtmlRenderer().GetHtml(blocks);
    }

    /// <summary>
    /// Load recursively the Children property of the given page and its child blocks recursively
    /// </summary>
    /// <param name="session"></param>
    /// <param name="page"></param>
    /// <param name="cancel"></param>
    /// <returns>the child blocks of the page</returns>
    public static async Task<List<Block>> LoadPageBlocks(this NotionSession session, Page page, CancellationToken cancel = default)
    {
        var blocks = await session.GetBlockChildren(page.Id, cancel: cancel)
            .Where(childBlock => BlockTypes.SupportedBlocks.Contains(childBlock.Type))
            .ToListAsync(cancel).ConfigureAwait(false);

        if (blocks.Count == 0)
            return new ();
        await session.LoadChildBlocks(blocks, cancel).ConfigureAwait(false);
        return blocks;
    }

    /// <summary>
    /// Loads recursively the Children property of the given blocks
    /// </summary>
    /// <param name="session"></param>
    /// <param name="blocks"></param>
    /// <param name="cancel"></param>
    public static async Task LoadChildBlocks(this NotionSession session, List<Block> blocks, CancellationToken cancel = default)
    {
        var blockWithChildren = new Queue<Block>(blocks.Where(b => b.HasChildren && BlockTypes.BlocksWithChildren.Contains(b.Type)));
        while (blockWithChildren.Count != 0)
        {
            var block = blockWithChildren.Dequeue();
            await session.GetChildren(block, cancel).ConfigureAwait(false);
            //recursive
            var children = block.Children.Where(b => b.HasChildren && BlockTypes.BlocksWithChildren.Contains(b.Type));
            foreach (var child in children)
                blockWithChildren.Enqueue(child);
        }
    }


    /// <summary>
    /// Create a syndication feed from the child pages of this page.
    /// </summary>
    /// <param name="session">a session</param>
    /// <param name="page">blocks of type ChildPage only</param>
    /// <param name="baseUrl">URL of the Notion domain for published pages</param>
    /// <param name="maxItems">max number of child pages to return</param>
    /// <param name="maxBlocks">limit the parsing of each page to the first maxBlocks blocks</param>
    /// <param name="stopBeforeFirstSubHeader">when true, stop parsing a page when a line containing a sub_header is found</param>
    /// <param name="cancel"></param>
    /// <returns>A SyndicationFeed containing one SyndicationItem per child page</returns>
    /// <remarks>
    /// Call this method with an existing page, which is the container for child pages.
    /// All child pages will go into this feed's items.
    /// 
    /// The created feed has the title and id of the page.
    /// 
    /// Are included only child pages which are not restricted with the "integration".
    /// </remarks>
    public static async Task<SyndicationFeed> GetSyndicationFeed(this NotionSession session, Page page, Uri baseUrl, int maxItems = 50, int maxBlocks = 20, bool stopBeforeFirstSubHeader = true, CancellationToken cancel = default)
    {
        var childPages = await session.GetBlockChildren(page.Id, cancel: cancel)
            .Where(b => b is { Type: BlockTypes.ChildPage, ChildPage: { } }) //2 checks are redundant. We could keep only one.
            .Take(maxItems)
            .ToListAsync(cancel).ConfigureAwait(false);

        if (childPages.Count == 0)
            return new() { LastUpdatedTime = DateTimeOffset.Now };
                    
        var feed = await session.GetSyndicationFeed(childPages, baseUrl, maxBlocks, stopBeforeFirstSubHeader, cancel).ConfigureAwait(false);
        
        var title = page.Title();
        
        //"workspace" data not available in API
        //feed.Id = space.Id.ToString("N");
        //feed.Title = new TextSyndicationContent(space.Name);
        //feed.Description = new TextSyndicationContent(space.Domain);
        feed.Id = page.Id;
        feed.Title = new (title.Title.FirstOrDefault()?.PlainText);
        //feed.Description = ??
        return feed;
    }

    /// <summary>
    /// Create a syndication feed from a list of page
    /// </summary>
    /// <param name="session">a session</param>
    /// <param name="childPages">blocks of type ChildPage only</param>
    /// <param name="baseUrl">URL of the Notion domain for published pages</param>
    /// <param name="maxBlocks">limits the parsing of each page to the first maxBlocks blocks</param>
    /// <param name="stopBeforeFirstSubHeader">when true, stop parsing a page when a line containing a sub_header is found</param>
    /// <param name="cancel"></param>
    /// <returns>A SyndicationFeed containing one SyndicationItem per page</returns>
    /// <remarks>
    /// The created feed has no title/description
    /// </remarks>
    public static async Task<SyndicationFeed> GetSyndicationFeed(this NotionSession session, List<Block> childPages, Uri baseUrl, int maxBlocks = 20, bool stopBeforeFirstSubHeader = true, CancellationToken cancel = default)
    {
        var feedItems = new List<SyndicationItem>();
        var htmlRenderer = new HtmlRenderer();

        //Load everything
        await session.LoadChildBlocks(childPages, cancel).ConfigureAwait(false);

        foreach (var page in childPages)
        {
            if (page.Type != BlockTypes.ChildPage)
                throw new ArgumentException("All childPages must be of type BlockTypes.ChildPage", nameof(childPages));

            //get blocks and extract an html content
            // var blocks = await session.GetBlockChildren(page.Id, cancel: cancel)
            //     .Take(maxBlocks)
            //     .ToListAsync(cancel).ConfigureAwait(false);
        
            var content = htmlRenderer.GetHtml(page.Children, stopBeforeFirstSubHeader);
            var title = page.ChildPage!.Title;
            var pageUri = NotionUtils.GetPageUri(page.Id, title, baseUrl);
    
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