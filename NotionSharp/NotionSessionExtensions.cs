﻿using System.ServiceModel.Syndication;
using System.Xml;
using NotionSharp.Lib;
using NotionSharp.Lib.ApiV3.Model;
using NotionSharp.Lib.ApiV3.Requests;
using NotionSharp.Lib.ApiV3.Results;

namespace NotionSharp;

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


    // public static async Task<LoadPageChunkResult> LoadPageChunk(this NotionSession session, Guid pageId, int chunkNumber = 0, int limit = 50, CancellationToken cancel = default)
    // {
    //     var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "loadPageChunk");
    //     var result = await request.PostJsonAsync(new LoadPageChunkRequest
    //     {
    //         ChunkNumber = chunkNumber,
    //         Cursor = new() { Stack = new () { new () { new () { Id = pageId, Index = chunkNumber, Table = "block" } } } },
    //         Limit = limit,
    //         PageId = pageId,
    //         VerticalColumns = false
    //     }, cancel);
    //
    //     //var s = result.GetStringAsync();
    //         
    //     return await result.GetJsonAsync<LoadPageChunkResult>();
    // }

    public static async Task<LoadPageChunkResult> LoadPageChunk(this NotionSession session, Guid pageId, Guid spaceId, int chunkNumber = 0, int limit = 30, CancellationToken cancel = default)
    {
        var request = session.HttpSession.CreateRequest(Constants.ApiBaseUrl + "loadCachedPageChunk");
        var result = await request.PostJsonAsync(new LoadCachedPageChunkRequest
        {
            Page = new () { Id = pageId, SpaceId = spaceId },
            Limit = limit,
            Cursor = new() { Stack = new () },
            ChunkNumber = chunkNumber,
            VerticalColumns = false
        }, cancel);

        return await result.GetJsonAsync<LoadPageChunkResult>();
    }

    public static async Task<SyndicationFeed> GetSyndicationFeed(this NotionSession session, int maxBlocks = 20, bool stopBeforeFirstSubHeader = true, CancellationToken cancel = default)
    {
        var userContent = await session.LoadUserContent(cancel);
        var space = userContent.RecordMap.Space.First().Value;

        //collection_view_page not supported
        var pages = space.Pages.Where(pageId => userContent.RecordMap.Block[pageId].Type == "page");

        var feed = await session.GetSyndicationFeed(space.Id, pages, maxBlocks, stopBeforeFirstSubHeader, cancel);
        feed.Id = space.Id.ToString("N");
        feed.Title = new (space.Name);
        feed.Description = new (space.Domain);
        return feed;
    }

    /// <summary>
    /// Create a syndication feed from a list of page
    /// </summary>
    /// <param name="session">a session</param>
    /// <param name="spaceId">the space in which to look</param>
    /// <param name="pages">the pages</param>
    /// <param name="maxBlocks">limits the parsing of each page to the 1st 20 blocks. Max value: 100</param>
    /// <param name="stopBeforeFirstSubHeader">when true, stop parsing a page when a line containing a sub_header is found</param>
    /// <param name="cancel"></param>
    /// <returns>A SyndicationFeed containing one SyndicationItem per page</returns>
    /// <remarks>
    /// The created feed has no title/description
    /// </remarks>
    public static async Task<SyndicationFeed> GetSyndicationFeed(this NotionSession session, Guid spaceId, IEnumerable<Guid> pages, int maxBlocks = 20, bool stopBeforeFirstSubHeader = true, CancellationToken cancel = default)
    {
        //notion's limitation
        if (maxBlocks > 100)
            throw new ArgumentOutOfRangeException(nameof(maxBlocks));

        var feedItems = new List<SyndicationItem>();
        foreach (var pageId in pages)
        {
            //get blocks and extract an html content
            var chunks = await session.LoadPageChunk(pageId, spaceId, 0, maxBlocks, cancel);
            var pageBlock = chunks.RecordMap.Block[pageId];

            //collection_view_page not supported
            if (pageBlock.Permissions?.Any(p => p.Role == Permission.RoleReader && p.Type == Permission.TypePublic) == true
                && pageBlock.Type == "page") 
            {
                //var content = chunks.RecordMap.GetHtmlAbstract(pageId);
                var content = chunks.RecordMap.GetHtml(pageId, throwIfBlockMissing: false, stopBeforeFirstSubHeader: stopBeforeFirstSubHeader, throwIfCantDecodeTextData: false);
                var pageUri = NotionUtils.GetPageUri(pageId, pageBlock.Title);

                var item = new SyndicationItem(pageBlock.Title, content, pageUri)
                {
                    Id = pageId.ToString("N"),
                    BaseUri = pageUri,
                    Summary = new TextSyndicationContent(content),
                    PublishDate = pageBlock.CreatedTime.EpochToDateTimeOffset(),
                    LastUpdatedTime = pageBlock.LastEditedTime.EpochToDateTimeOffset(),
                };

                if (!String.IsNullOrWhiteSpace(pageBlock.Format?.PageIcon))
                {
                    if(Uri.TryCreate(pageBlock.Format.PageIcon, UriKind.Absolute, out _))
                        item.AttributeExtensions.Add(new XmlQualifiedName("iconUrl"), pageBlock.Format.PageIcon);
                    else
                        item.AttributeExtensions.Add(new XmlQualifiedName("iconString"), pageBlock.Format.PageIcon);
                }

                feedItems.Add(item);
            }
        }

        var feed = new SyndicationFeed(feedItems)
        {
            LastUpdatedTime = feedItems.DefaultIfEmpty().Max(item => item?.LastUpdatedTime ?? DateTimeOffset.MinValue),
            //Copyright = 
        };

        return feed;
    }
}