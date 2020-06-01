using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NotionSharp;
using NotionSharp.Lib.ApiV3.Results;

namespace NotionSharpTest
{
    [TestClass]
    public class TestNotionHtml
    {
        [TestMethod]
        public void TestGetHtml()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData", "LoadPageChunkResult1.json"));
            var chunks = JsonConvert.DeserializeObject<LoadPageChunkResult>(json);
            Assert.IsNotNull(chunks);

            var content = chunks.RecordMap.GetHtml(throwIfBlockMissing: false);
            Assert.IsNotNull(content);
            Assert.IsTrue(content.Length > 5000);
        }

        [TestMethod]
        public void TestGetHtmlAbstract()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData", "LoadPageChunkResult1.json"));
            var chunks = JsonConvert.DeserializeObject<LoadPageChunkResult>(json);
            Assert.IsNotNull(chunks);
            var content = chunks.RecordMap.GetHtmlAbstract();
            Assert.IsNotNull(content);
            Assert.AreEqual(@"<p class=""notion_text"">Creating a good Xamarin Forms control - Part 3 - UI Day 4</p>
<p class=""notion_text"">In <a href=""https://medium.com/@bigoudi/creating-a-good-xamarin-forms-control-part-2-ui-day-3-688bd0b3333d"">the previous article</a> I proposed the foundations of a win-win architecture for a good Xamarin Forms control using a multi targeting project. </p>
<p class=""notion_text"">Today I am presenting a way to create a control with a renderer that auto register itself, greatly simplifying the control&#39;s usage in teams, but also its documentation and its maintenance.</p>
<p class=""notion_image""><img src=""https://www.notion.so/image/https%3A%2F%2Fs3-us-west-2.amazonaws.com%2Fsecure.notion-static.com%2F6ea6e3a6-2a17-44f1-a25f-7e09b6114035%2Fdownload.png""/></p>
", content);
        }
        
        [TestMethod]
        public void TestGetHtml_BlogTestPage1()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData", "BlogTestPage1.json"));
            var chunks = JsonConvert.DeserializeObject<LoadPageChunkResult>(json);
            Assert.IsNotNull(chunks);

            var content = chunks.RecordMap.GetHtml(throwIfBlockMissing: false);
            Assert.IsNotNull(content);
            Assert.IsTrue(content.StartsWith(@"<p class=""notion_text"">Creating a good Xamarin Forms control - Part 3 - UI Day 4"));
        }

        [TestMethod]
        public void TestGetHtml_SubBullets()
        {
            var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData", "SubBullets.json"));
            var chunks = JsonConvert.DeserializeObject<LoadPageChunkResult>(json);
            Assert.IsNotNull(chunks);

            var content = chunks.RecordMap.GetHtml(throwIfBlockMissing: false);
            Assert.IsNotNull(content);
            Assert.IsTrue(content.StartsWith(@"<h1 class=""notion_header"">⚡Welcome ⚡</h1>
<p class=""notion_text"">Here at Vapolia we are fond of coding. With 25 years of experience within small and large companies, we are particulary good at understanding your needs !</p>
<p class=""notion_text""></p>
<p class=""notion_text"">&#129311; Our main services:</p>
<p class=""notion_text""></p>
<ul><li class=""notion_bulleted_list"">Gather your ideas into an understandable specification</li>
<p class=""notion_bulleted_list"">
<p class=""notion_text"">&#127774; we convert your vision into a specification understandable by everyone - from the product owner to the developer&#39;s team.</p>
<p class=""notion_text""></p>
</div></ul>
<ul><li class=""notion_bulleted_list"">You need a mobile app</li>
"));
        }
    }
}
