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

        [TestMethod]
        public async Task TestGetRssFeed()
        {
            var sessionInfo = GetTestInfo();
            var session = new NotionSession(sessionInfo);
            var feed = await session.GetSyndicationFeed();
            Assert.IsNotNull(feed);
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
                TokenV2 = "f8bd7892dcdaa690df1dc36f2a4e306b23563564d9950a734460868e878c36d835868f79bc649faca0a40885b26c4886fa57c9ecc0e3413719ef17128c941c2fb62dbdf7533cf427f39e22ed5449",
                NotionBrowserId = Guid.Parse("deff37ea-82a6-4a67-a112-00dc73043c75"),
                NotionUserId = Guid.Parse("ab9257e1-d027-4494-8792-71d90b63dd35")
            };
        }
    }
}
