using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NotionSharp;
using NotionSharp.Lib;
using NotionSharp.Lib.ApiV3.Model;
using NotionSharp.Lib.ApiV3.Results;

namespace NotionSharpTest
{
    [TestClass]
    public class TestNotionApiV3
    {
        [TestMethod]
        public async Task TestGetUserTasks()
        {
            var sessionInfo = GetTestInfo();
            var session = new NotionSession(sessionInfo);

            var userTasks = await session.GetUserTasks(new object());
            Assert.IsNotNull(userTasks);

            var clientExperiments = await session.GetClientExperiments(Guid.Parse("53d14533-94b9-4e87-aec8-9378b2bcedcc"));
            Assert.IsNotNull(clientExperiments);
            Assert.IsTrue(clientExperiments.Experiments.Count > 0);

            var userContent = await session.LoadUserContent();
            Assert.IsNotNull(userContent);
        }

        [TestMethod]
        public async Task TestLoadPageChunkResult()
        {
            var sessionInfo = GetTestInfo();
            var session = new NotionSession(sessionInfo);

            var pageChunk = await session.LoadPageChunk(Guid.Parse("4e4999b4-161a-449d-bbd1-bdbce690c7cb"), 0, 50);
            Assert.IsNotNull(pageChunk);
            Assert.IsTrue(pageChunk.RecordMap.Block.Count > 0);
        }


        [TestMethod]
        public void TestGetHtml()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData", "LoadPageChunkResult1.json"));
            var chunks = JsonConvert.DeserializeObject<LoadPageChunkResult>(json);
            Assert.IsNotNull(chunks);
            var content = chunks.RecordMap.GetHtml(throwIfBlockMissing: false);
            Assert.IsNotNull(content);
            Assert.AreEqual(content.Length, 5428);
        }

        [TestMethod]
        public void TestGetHtmlAbstract()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData", "LoadPageChunkResult1.json"));
            var chunks = JsonConvert.DeserializeObject<LoadPageChunkResult>(json);
            Assert.IsNotNull(chunks);
            var content = chunks.RecordMap.GetHtmlAbstract();
            Assert.IsNotNull(content);
            Assert.AreEqual(@"<p>Creating a good Xamarin Forms control - Part 3 - UI Day 4
</p>
<p>In 
<a href=""https://medium.com/@bigoudi/creating-a-good-xamarin-forms-control-part-2-ui-day-3-688bd0b3333d"">the previous article</a> I proposed the foundations of a win-win architecture for a good Xamarin Forms control using a multi targeting project. 
</p>
<p>Today I am presenting a way to create a control with a renderer that auto register itself, greatly simplifying the control&#39;s usage in teams, but also its documentation and its maintenance.
</p>
<p></p>
", content);
        }

        /// <summary>
        /// get root pages of 1st space and put them in a RSS feed
        /// </summary>
        [TestMethod]
        public async Task GetRssFeedFromRootPagesOfFirstSpace()
        {
            var sessionInfo = GetTestInfo();
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
            var sessionInfo = GetTestInfo();
            var session = new NotionSession(sessionInfo);

            //Get first space
            var userContent = await session.LoadUserContent();
            var space = userContent.RecordMap.Space.First().Value;
            //Get sub-pages of first page
            var firstPage = userContent.RecordMap.Block[space.Pages[0]];
            var subPages = firstPage.Content;

            var feedPublicBlog = await session.GetSyndicationFeed(subPages);
            feedPublicBlog.Title = new TextSyndicationContent(firstPage.Title);
            Assert.IsNotNull(feedPublicBlog);   
            Assert.IsTrue(feedPublicBlog.Items.Any());
        }

        [TestMethod]
        public void TestEpoch()
        {
            Assert.AreEqual(new DateTimeOffset(2020,03,16,15,25,0, TimeSpan.Zero), 1584372300000.EpochToDateTimeOffset());
        }

        private NotionSessionInfo GetTestInfo()
        {
            //notioncsharp@yopmail.com
            //http://yopmail.com/notioncsharp
            return new NotionSessionInfo
            {
                TokenV2 = "198f2adfc90e1a91d7b8cc8a63272e1cc75ef9bfdacd35f9e0a3fcf3b28cd817ec427f72be3c2634c75c7f4401d6061c13852725c855c844b6bf8d2c677ac25c52d5ce76caa3c3bc215487587c7c",
                NotionBrowserId = Guid.Parse("f8a52372-15da-494f-ae3f-54ea2f80fe6b"),
                NotionUserId = Guid.Parse("ab9257e1-d027-4494-8792-71d90b63dd35")
            };
        }
    }
}
