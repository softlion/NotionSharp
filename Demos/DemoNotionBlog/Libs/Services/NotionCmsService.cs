using System.Reactive.Subjects;
using System.ServiceModel.Syndication;
using Flurl.Http;
using Hangfire;
using Microsoft.Extensions.Options;
using NotionSharp;

namespace DemoNotionBlog.Libs.Services
{
    public static class NotionCmsServiceExtensions
    {
        public static string? ToLink(this string? title)
        {
            if(title != null)
                return Uri.EscapeDataString(title.Replace(" ", "_").ToLower());
            return null;
        }
    }

    public class NotionCmsServiceUpdateJob
    {
        private readonly NotionCmsService service;

        public NotionCmsServiceUpdateJob(NotionCmsService service)
        {
            this.service = service;
        }

        public void Refresh()
        {
            _ = service.RefreshNotionData();
        }
    }

    public class NotionCmsService
    {
        private readonly ILogger<NotionCmsService> logger;
        private readonly IOptionsMonitor<NotionOptions> notionOptions;

        public BehaviorSubject<List<SyndicationItem>?> CmsArticles { get; }
        public string? CmsTitle { get; set; }

        public NotionCmsService(ILogger<NotionCmsService> logger, IOptionsMonitor<NotionOptions> notionOptions)
        {
            this.logger = logger;
            this.notionOptions = notionOptions;
            notionOptions.OnChange(options => _ = RefreshNotionData());

            //If you crash here, make sure you either:
            //- updated the notion session keys in appsettings-secrets.json
            //- or created user secrets :
            //  dotnet user-secrets init
            //  dotnet user-secrets set "Notion:Key" "xxXxxXXxxXxxxXXxxx...xxXxxX"
            //  dotnet user-secrets set "Notion:BrowserId" "aabbccdd-aabb-aabb-aabb-aabbccddaabb"
            //  dotnet user-secrets set "Notion:UserId" "eeffeeff-eeff-eeff-eeff-eeffeeffeeff"

            //Load Startup Data
            CmsArticles = new BehaviorSubject<List<SyndicationItem>?>(null);

            Task.Run(() => _ = RefreshNotionData().ConfigureAwait(false));
            RecurringJob.AddOrUpdate<NotionCmsServiceUpdateJob>(swe => swe.Refresh(), Cron.HourInterval(4));
        }

        private bool isRefreshing;

        public async Task RefreshNotionData()
        {
            if(isRefreshing)
                return;
            isRefreshing = true;
            
            var option = notionOptions?.CurrentValue;
            logger.LogInformation($"refreshing notion data for title '{option?.CmsPageTitle}'");
            if (String.IsNullOrWhiteSpace(option?.Key))
            {
                logger.LogError($"Can not refresh notion data for title '{option?.CmsPageTitle}': key is missing");
                return;
            }
            if (option.BrowserId == Guid.Empty)
            {
                logger.LogError($"Can not refresh notion data for title '{option?.CmsPageTitle}': browserId is missing");
                return;
            }
            if (option.UserId == Guid.Empty)
            {
                logger.LogError($"Can not refresh notion data for title '{option?.CmsPageTitle}': userId is missing");
                return;
            }

            try
            {
                var notionSession = new NotionSession(new NotionSessionInfo { TokenV2 = option.Key, NotionBrowserId = option.BrowserId, NotionUserId = option.UserId });
                var userContent = await notionSession.LoadUserContent().ConfigureAwait(false);
                var spacePages = userContent.RecordMap.Space.First().Value.Pages;

                //Refresh CMS
                var cmsPageId = spacePages.First(pageId => userContent.RecordMap.Block.TryGetValue(pageId, out var page) && page.Title == option.CmsPageTitle);
                var cmsPages = userContent.RecordMap.Block[cmsPageId].Content;
                var cmsItems = await notionSession.GetSyndicationFeed(cmsPages, maxBlocks: 100, stopBeforeFirstSubHeader: false).ConfigureAwait(false);
                CmsTitle = userContent.RecordMap.Block[cmsPageId].Title;
                CmsArticles.OnNext(cmsItems.Items.ToList());
            }
            catch (FlurlHttpException e) when(e.StatusCode == 401)
            {
                logger.LogError(e, $"Invalid notion credentials for page {option?.CmsPageTitle}");
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Can't update notion data or find page {option?.CmsPageTitle}");
            }
            finally
            {
                isRefreshing = false;
            }
        }
    }
}
