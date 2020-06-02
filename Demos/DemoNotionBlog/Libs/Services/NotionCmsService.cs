﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotionSharp;

namespace DemoNotionBlog.Libs.Services
{
    public static class NotionCmsServiceExtensions
    {
        public static string ToLink(this string title)
        {
            if(title != null)
                return Uri.EscapeDataString(title.Replace(" ", "_").ToLower());
            return null;
        }
    }

    public class NotionCmsService
    {
        private readonly ILogger<NotionCmsService> logger;
        private readonly NotionOptions notionOptions;

        public BehaviorSubject<List<SyndicationItem>> CmsArticles { get; }

        public NotionCmsService(ILogger<NotionCmsService> logger, IOptions<NotionOptions> notionOptions)
        {
            this.logger = logger;
            this.notionOptions = notionOptions.Value;

            //Load Startup Data
            CmsArticles = new BehaviorSubject<List<SyndicationItem>>(null);

            Task.Run(() => _ = RefreshNotionData().ConfigureAwait(false));
        }

        private bool isRefreshing;

        public async Task RefreshNotionData()
        {
            if(isRefreshing)
                return;
            isRefreshing = true;

            try
            {
                var notionSession = new NotionSession(new NotionSessionInfo {TokenV2 = notionOptions.Key, NotionBrowserId = notionOptions.BrowserId, NotionUserId = notionOptions.UserId });
                var userContent = await notionSession.LoadUserContent().ConfigureAwait(false);
                var spacePages = userContent.RecordMap.Space.First().Value.Pages;

                //Refresh CMS
                var cmsPageId = spacePages.First(pageId => userContent.RecordMap.Block[pageId].Title == notionOptions.CmsPageTitle);
                var cmsPages = userContent.RecordMap.Block[cmsPageId].Content;
                var cmsItems = await notionSession.GetSyndicationFeed(cmsPages, 100000).ConfigureAwait(false);
                CmsArticles.OnNext(cmsItems.Items.ToList());
            }
            catch (Exception e)
            {
                logger.LogError(e, "Can't update notion data");
            }
            finally
            {
                isRefreshing = false;
            }
        }
    }
}