using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp;

namespace NotionSharpTest
{
    [TestClass]
    public class TestNotionRss
    {
        /// <summary>
        /// get root pages of 1st space and put them in a RSS feed
        /// </summary>
        [TestMethod]
        public async Task GetRssFeedFromRootPagesOfFirstSpace()
        {
            var sessionInfo = TestUtils.CreateNotionSessionInfo();
            var session = new NotionSession(sessionInfo);

            var feed = await session.GetSyndicationFeed();
            Assert.IsNotNull(feed);
            Assert.IsTrue(feed.Items.Any());
        }


        /// <summary>
        /// get 1st page of 1st space, then get all sub-pages of this page and put them in a RSS feed
        /// </summary>
        [TestMethod]
        public async Task GetRssFeedFromSubPagesOfFirstPageOfFirstSpace()
        {
            //Connect to the Notion account
            var sessionInfo = TestUtils.CreateNotionSessionInfo();
            var notionSession = new NotionSession(sessionInfo);

            //Get first notion "space"
            var userContent = await notionSession.LoadUserContent();
            var spacePages = userContent.RecordMap.Space.First().Value.Pages;

            //Get sub-pages of the "public blog" page, which makes the content of the blog
            var blogId = spacePages.First(pageId => userContent.RecordMap.Block[pageId].Title == "Public blog");
            var blog = userContent.RecordMap.Block[blogId];
            var blogPages = blog.Content;

            //Convert the blogPages into a RSS feed
            var feed = await notionSession.GetSyndicationFeed(blogPages);
            feed.Title = new TextSyndicationContent(blog.Title);

            Assert.IsNotNull(feed);
            Assert.IsTrue(feed.Items.Any());
        }


    }
}
