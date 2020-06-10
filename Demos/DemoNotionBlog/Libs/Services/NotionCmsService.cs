using System;
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
        public string CmsTitle { get; set; }

        public NotionCmsService(ILogger<NotionCmsService> logger, IOptions<NotionOptions> notionOptions)
        {
            this.logger = logger;

            //If you crash here, make sure you either:
            //- updated the notion session keys in appsettings-secrets.json
            //- or created user secrets :
            //  dotnet user-secrets init
            //  dotnet user-secrets set "Notion:Key" "xxXxxXXxxXxxxXXxxx...xxXxxX"
            //  dotnet user-secrets set "Notion:BrowserId" "aabbccdd-aabb-aabb-aabb-aabbccddaabb"
            //  dotnet user-secrets set "Notion:UserId" "eeffeeff-eeff-eeff-eeff-eeffeeffeeff"
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
                var cmsItems = await notionSession.GetSyndicationFeed(cmsPages, maxBlocks: 100000, stopBeforeFirstSubHeader: false).ConfigureAwait(false);
                CmsTitle = userContent.RecordMap.Block[cmsPageId].Title;
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
