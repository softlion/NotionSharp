using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp.ApiClient.Lib;
using NotionSharp.ApiClient.Lib.HtmlRendering;
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

        var allBlocks = await session.GetBlockChildren(page.Id).ToListAsync();

        var blocks = allBlocks
            .Where(childBlock => BlockTypes.SupportedBlocks.Contains(childBlock.Type))
            .ToList();
        Assert.AreNotEqual(0, blocks.Count);       

        var unsupportedBlocks = allBlocks
            .Where(childBlock => !BlockTypes.SupportedBlocks.Contains(childBlock.Type))
            .ToList();
        Assert.AreEqual(0, unsupportedBlocks.Count);       
        
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
    }

    [TestMethod]
    public async Task TestGetHtml_Link()
    {
        var blocksJson = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.full.children.json"));
        var blocks = JsonSerializer.Deserialize<List<Block>>(blocksJson, HttpNotionSession.NotionJsonSerializationOptions);

        var expectedFilePath = "AboutThis.blocks.link.html";
        var blockIndex = 0;

        var block = blocks.Skip(blockIndex).Take(1).ToList();
        var html = new HtmlRenderer().GetHtml(block);
        var expectedHtml = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "JsonData", expectedFilePath));
        Assert.AreEqual(expectedHtml, html);
    }
    
    [TestMethod]
    public async Task TestGetHtml_Title1()
    {
        var blocksJson = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.full.children.json"));
        var blocks = JsonSerializer.Deserialize<List<Block>>(blocksJson, HttpNotionSession.NotionJsonSerializationOptions);

        var blockIndex = 2;

        var block = blocks.Skip(blockIndex).Take(1).ToList();
        var html = new HtmlRenderer().GetHtml(block);
        var expectedHtml = """<h1><div class="notion-line">Title 1</div></h1>""" + "\r\n";
        Assert.AreEqual(expectedHtml, html);
    }

    [TestMethod]
    public async Task TestGetHtml_Title2()
    {
        var blocksJson = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.full.children.json"));
        var blocks = JsonSerializer.Deserialize<List<Block>>(blocksJson, HttpNotionSession.NotionJsonSerializationOptions);

        var blockIndex = 3;

        var block = blocks.Skip(blockIndex).Take(1).ToList();
        var html = new HtmlRenderer().GetHtml(block);
        var expectedHtml = """<h2><div class="notion-line">Title 2</div></h2>""" + "\r\n";
        Assert.AreEqual(expectedHtml, html);
    }

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