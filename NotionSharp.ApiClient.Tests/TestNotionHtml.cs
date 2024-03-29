﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        var blocks = JsonSerializer.Deserialize<List<Block>>(blocksJson, HttpNotionSession.NotionJsonFullSerializationOptions);

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
        var blocks = JsonSerializer.Deserialize<List<Block>>(blocksJson, HttpNotionSession.NotionJsonFullSerializationOptions);

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
        var blocks = JsonSerializer.Deserialize<List<Block>>(blocksJson, HttpNotionSession.NotionJsonFullSerializationOptions);

        var blockIndex = 3;

        var block = blocks.Skip(blockIndex).Take(1).ToList();
        var html = new HtmlRenderer().GetHtml(block);
        var expectedHtml = """<h2><div class="notion-line">Title 2</div></h2>""" + "\r\n";
        Assert.AreEqual(expectedHtml, html);
    }
    
    [TestMethod]
    public async Task TestGetHtml_Bold()
    {
        var blocksJson = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.full.children.json"));
        var blocks = JsonSerializer.Deserialize<List<Block>>(blocksJson, HttpNotionSession.NotionJsonFullSerializationOptions);

        var blockIndex = 4;

        var block = blocks.Skip(blockIndex).Take(1).ToList();
        var html = new HtmlRenderer().GetHtml(block);
        var expectedHtml = """<div class="notion-paragraph"><div class="notion-line"><span class=" notion-bold">Bold text</span></div></div>""" + "\r\n";
        Assert.AreEqual(expectedHtml, html);
    }
    
        
    [TestMethod]
    public async Task TestGetHtml_Bullet_Levels2()
    {
        var blocksJson = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.full.children.json"));
        var blocks = JsonSerializer.Deserialize<List<Block>>(blocksJson, HttpNotionSession.NotionJsonFullSerializationOptions);

        var blockIndex = 5;

        var block = blocks.Skip(blockIndex).Take(1).ToList();
        var html = new HtmlRenderer().GetHtml(block);
        var expectedHtml = @"<ul><li><div class=""notion-line"">Level1</div><ul><li><div class=""notion-line"">Level 2</div><ul><li><div class=""notion-line"">Level 3</div><ul><li><div class=""notion-line"">Level 4</div><ul><li><div class=""notion-line"">Level 5</div><ul><li><div class=""notion-line"">Level 6</div><ul><li><div class=""notion-line"">Level 7</div></li></ul>
</li></ul>
</li></ul>
</li></ul>
</li></ul>
</li></ul>
</li></ul>
";
        Assert.AreEqual(expectedHtml, html);
    }

    /// <summary>
    /// Test Code blocks
    /// </summary>
    /// <remarks>
    /// AppendLine appends \r\n on windows, \r on linux 
    /// </remarks>
    [TestMethod]
    public async Task TestGetHtml_Code()
    {
        var blocksJson = await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.full.children.json"));
        var blocks = JsonSerializer.Deserialize<List<Block>>(blocksJson, HttpNotionSession.NotionJsonFullSerializationOptions);

        var block = blocks.Where(b => b.Type == "code").Take(1).ToList();
        Assert.AreEqual(1, block.Count);

        var html = new HtmlRenderer().GetHtml(block).Replace("\r", "");
        var expectedHtml = """
<div class="notion-code-block" /><code class="language-c#">
&lt;svg:SvgImage Svg=&quot;res:images.logo&quot; HeighRequest=&quot;32&quot; /&gt;

XamSvg.Shared.Config.ResourceAssemblies = new [] { typeof(App).Assembly };
</code>
</div>

""".Replace("\r", "");

        var i = FindDifferingIndex(expectedHtml, html);
        Assert.AreEqual(-1, i);
        
        Assert.AreEqual(expectedHtml, html);
    }

    static int FindDifferingIndex(string str1, string str2)
    {
        var minLength = Math.Min(str1.Length, str2.Length);

        for (var i = 0; i < minLength; i++)
        {
            if (str1[i] != str2[i])
                return i; // Found the differing index
        }

        // If the loop completes, check if one string is longer than the other
        if (str1.Length != str2.Length)
            return minLength; // Strings are different in length

        return -1; // Strings are identical
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