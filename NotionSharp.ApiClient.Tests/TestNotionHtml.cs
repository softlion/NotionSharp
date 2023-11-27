﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp.ApiClient.Tests.Lib;

namespace NotionSharp.ApiClient.Tests;

[TestClass]
public class TestNotionHtml
{
    [TestMethod]
    public async Task TestGetPage()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        //Get any page returned by search
        var page = await session.Search(query: "About this", filterOptions: FilterOptions.ObjectPage).FirstAsync();
        Assert.IsNotNull(page);
        Assert.IsNotNull(page.Id);
        Assert.IsNotNull(page.Properties);
        Assert.IsNotNull(page.Parent);
        Assert.AreEqual("About this", page.Title().Title[0].PlainText);

        var blocks = await session.GetBlockChildren(page.Id)
            .Where(childBlock => BlockTypes.SupportedBlocks.Contains(childBlock.Type))
            .ToListAsync().ConfigureAwait(false);
        
        var blockWithChildren = new Queue<Block>(blocks.Where(b => b.HasChildren && BlockTypes.BlocksWithChildren.Contains(b.Type)));
        while (blockWithChildren.Count != 0)
        {
            var block = blockWithChildren.Dequeue();
            await session.GetChildren(block);
            //recursive
            var children = block.Children.Where(b => b.HasChildren && BlockTypes.BlocksWithChildren.Contains(b.Type));
            foreach (var child in children)
                blockWithChildren.Enqueue(child);
        }

        var expectedHtml = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.expected.html"));
        var html = await session.GetHtml(page);
        //Assert.AreEqual(expectedHtml, html);
    }
    
//     [TestMethod]
//     public void TestGetHtml()
//     {
//         var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "JsonData", "LoadPageChunkResult1.json"));
//         var chunks = JsonSerializer.Deserialize<LoadPageChunkResult>(json, JsonTextSerializerOptions.Options);
//         Assert.IsNotNull(chunks);
//         Assert.IsNotNull(chunks.RecordMap);
//         Assert.IsNotNull(chunks.RecordMap.Block.First().Value.TheValue);
//
//         var content = chunks.RecordMap.GetHtml(throwIfBlockMissing: false);
//         Assert.IsNotNull(content);
//         Assert.IsTrue(content.Length > 5000);
//     }
//
//     [TestMethod]
//     public void TestGetHtmlAbstract()
//     {
//         var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData", "LoadPageChunkResult1.json"));
//         var chunks = JsonSerializer.Deserialize<LoadPageChunkResult>(json, JsonTextSerializerOptions.Options);
//         Assert.IsNotNull(chunks);
//         var content = chunks.RecordMap.GetHtmlAbstract();
//         Assert.IsNotNull(content);
//         Assert.AreEqual(@"<div class=""notion-text-block"">Creating a good Xamarin Forms control - Part 3 - UI Day 4</div>
// <div class=""notion-text-block"">In <a href=""https://medium.com/@bigoudi/creating-a-good-xamarin-forms-control-part-2-ui-day-3-688bd0b3333d"">the previous article</a> I proposed the foundations of a win-win architecture for a good Xamarin Forms control using a multi targeting project. </div>
// <div class=""notion-text-block"">Today I am presenting a way to create a control with a renderer that auto register itself, greatly simplifying the control&#39;s usage in teams, but also its documentation and its maintenance.</div>
// <div class=""notion-image-block""><img style=""width:347px"" src=""https://www.notion.so/image/https%3A%2F%2Fs3-us-west-2.amazonaws.com%2Fsecure.notion-static.com%2F6ea6e3a6-2a17-44f1-a25f-7e09b6114035%2Fdownload.png?table=block&id=32297a2f-7ff2-4478-b182-2db36ab63878&userId=?table=block&id=32297a2f-7ff2-4478-b182-2db36ab63878&cache=v2""/></div>
// ", content);
//     }
//         
//     [TestMethod]
//     public void TestGetHtml_BlogTestPage1()
//     {
//         var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData", "BlogTestPage1.json"));
//         var chunks = JsonSerializer.Deserialize<LoadPageChunkResult>(json, JsonTextSerializerOptions.Options);
//         Assert.IsNotNull(chunks);
//
//         var content = chunks.RecordMap.GetHtml(throwIfBlockMissing: false);
//         Assert.IsNotNull(content);
//         Assert.IsTrue(content.StartsWith(@"<div class=""notion-text-block"">Creating a good Xamarin Forms control - Part 3 - UI Day 4</div>"));
//     }
//         
//     [TestMethod]
//     public void TestGetHtml_BlogTestPage1new()
//     {
//         var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData", "BlogTestPage1new.json"));
//         var chunks = JsonSerializer.Deserialize<LoadPageChunkResult>(json, JsonTextSerializerOptions.Options);
//         Assert.IsNotNull(chunks);
//
//         var content = chunks.RecordMap.GetHtml(throwIfBlockMissing: false);
//         Assert.IsNotNull(content);
//         Assert.IsTrue(content.Contains(@"class=""notion-text-block notion-image-caption"">A V</div>"));
//     }
//
//     [TestMethod]
//     public void TestGetHtml_SubBullets()
//     {
//         var json = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"..\..\..\JsonData", "SubBullets.json"));
//         var chunks = JsonSerializer.Deserialize<LoadPageChunkResult>(json, JsonTextSerializerOptions.Options);
//         Assert.IsNotNull(chunks);
//
//         var content = chunks.RecordMap.GetHtml(throwIfBlockMissing: false);
//         Assert.IsNotNull(content);
//         Assert.IsTrue(content.StartsWith(@"<h1 class=""notion-header-block"">⚡Welcome ⚡</h1>
// <div class=""notion-text-block"">Here at Vapolia we are fond of coding. With 25 years of experience within small and large companies, we are particulary good at understanding your needs !</div>
// <div class=""notion-text-block""></div>
// <div class=""notion-text-block"">&#129311; Our main services:</div>
// <div class=""notion-text-block""></div>
// <ul class=""notion-bulleted_list-block""><li>Gather your ideas into an understandable specification</li>
// <div class=""notion-text-block"">&#127774; we convert your vision into a specification understandable by everyone - from the product owner to the developer&#39;s team.</div>
// <div class=""notion-text-block""></div>
// </ul>
// <ul class=""notion-bulleted_list-block""><li>You need a mobile app</li>
// <div class=""notion-text-block"">&#128241; let us create it for you, we use the lastest cross platform technologies. You want it connected to the cloud? No problem!</div>
// <div class=""notion-text-block""></div>
// </ul>
// "));
//     }

}