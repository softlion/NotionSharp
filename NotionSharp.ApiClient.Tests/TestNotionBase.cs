using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp.ApiClient.Lib;
using NotionSharp.ApiClient.Tests.Lib;

namespace NotionSharp.ApiClient.Tests;

[TestClass]
public class TestNotionBase
{
    /// <summary>
    /// Automatic paging
    /// </summary>
    [TestMethod]
    public async Task TestSearch()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());

        var totalItems = 0;
        List<Page> pages = new ();
        await foreach (var item in session.Search(pageSize: 2))
        {
            totalItems++;
            pages.Add(item);
        }

        Assert.AreEqual(3, totalItems);
    }

    /// <summary>
    /// Manual paging (obsolete)
    /// </summary>
    [TestMethod]
    public async Task TestSearchPaged()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());

        var pagingOptions = new PagingOptions {PageSize = 2};
        var searchResult = await session.SearchPaged(pagingOptions: pagingOptions);
        Assert.IsNotNull(searchResult?.Results);

        var totalItems = searchResult.Results.Count;
        var i = 10;
        while (searchResult.HasMore && i-- > 0)
        {
            pagingOptions.StartCursor = searchResult.NextCursor;
            searchResult = await session.SearchPaged(pagingOptions: pagingOptions);
            totalItems += searchResult.Results.Count;
        }
        Assert.AreNotEqual(0, i);
        Assert.AreEqual(3, totalItems);
    }

    [TestMethod]
    public async Task TestGetUsers()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        var users = session.GetUsers();
        Assert.IsNotNull(users);
        int i, iBot, iPerson;
        i = iBot = iPerson = 0;
        await foreach (var user in users)
        {
            i++;
            if (user.UserType == UserTypeConst.Bot)
                iBot++;
            if (user.UserType == UserTypeConst.Person)
                iPerson++;
        }
            
        Assert.AreEqual(2, i);
        Assert.AreEqual(1, iBot);
        Assert.AreEqual(1, iPerson);
    }
        
    [TestMethod]
    public async Task TestGetUser()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        var user1a = await session.GetUsers().FirstAsync();
        Assert.IsNotNull(user1a);
            
        var user1b = await session.GetUser(user1a.Id);
            
        Assert.IsTrue(user1a.Equals(user1b));
    }
        
    [TestMethod]
    public async Task TestGetPage()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        //Get any page returned by search
        var page = await session.Search(pageSize: 1, filterOptions: FilterOptions.ObjectPage).FirstAsync();
        Assert.IsNotNull(page);

        Assert.IsNotNull(page.Id);

        //Get the page details
        var pageProperties = await session.GetPage(page.Id);
        Assert.IsNotNull(pageProperties);
        Assert.IsNotNull(pageProperties.Parent);
        // Assert.IsNotNull(pageProperties.Properties);
        // var title = pageProperties.Title;
        // Assert.IsNotNull(title);
        // Assert.IsNotNull(title.Title);
        // Assert.AreNotEqual(0, title.Title.Count);
        var id = pageProperties.Id;
    }

    [TestMethod]
    public async Task TestGetBlock()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        var blockId = "18dfbe55-5d7c-416e-9485-7855d4a3949e"; //from TestGetPage()
        await foreach (var block in session.GetBlockChildren(blockId))
        {
            Assert.IsNotNull(block.Type);
            switch (block.Type)
            {
                case BlockTypes.Paragraph:
                    Assert.IsNotNull(block.Paragraph);
                    break;

                case BlockTypes.ChildPage:
                    Assert.IsNotNull(block.ChildPage);
                    break;
                    
                default:
                    break;
            }
        }
    }

    [TestMethod]
    public async Task TestJsonCase()
    {
        var searchRequest = new SearchRequest { Query = "theQuery", Sort = SortOptions.LastEditedTimeDescending, 
            Filter = new () { Property = "page" }, PageSize = 50 };
        var jsonString = JsonSerializer.Serialize(searchRequest, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.AreEqual(  """{"query":"theQuery","sort":{"direction":1,"timestamp":0},"filter":{"property":"page"},"page_size":50}""", jsonString);
    }
    
    [TestMethod]
    public async Task TestGetBlockChildrenJson()
    {
        var getBlockChildren = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "JsonData", "GetBlockChildren.json"));
        var json = JsonSerializer.Deserialize<PaginationResult<Block>>(getBlockChildren, HttpNotionSession.NotionJsonSerializationOptions);
        
        Assert.IsNotNull(json);

        var blockFile = json.Results.First(b => b.Type == "file");
        Assert.AreEqual("https://testFileUrl", blockFile.File.File.Url);
    }

    [TestMethod]
    public async Task TestJsonPolymorphicDeserialization()
    {
        var searchPageResultJson = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "JsonData", "SearchPageResult.json"));
        var json = JsonSerializer.Deserialize<PaginationResult<Page>>(searchPageResultJson, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(json);
        Assert.IsNotNull(json.Results);
        Assert.AreEqual(3, json.Results.Count);
        
        var page = json.Results[0];
        Assert.IsNotNull(page.Id);
        Assert.IsNotNull(page.Url);
        Assert.AreEqual("https://wise-spirit-737.notion.site/Procrastination-4e4999b4161a449dbbd1bdbce690c7cb",page.PublicUrl);
        Assert.IsNotNull(page.Title());
        Assert.AreNotEqual(0, page.Title()!.Title.Count);
        Assert.AreEqual("Procrastination", page.Title()!.Title[0].PlainText);
        Assert.AreEqual("Procrastination", page.Title()!.Title[0].Text!.Content);

        page = json.Results[1];
        Assert.IsNotNull(page.Id);
        Assert.IsNotNull(page.Url);
        Assert.IsNotNull(page.Title());
        Assert.AreEqual("Public blog", page.Title()!.Title[0].PlainText);
    }

        
    [TestMethod]
    public async Task TestGetTopLevelPages()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        var pages = await session.GetTopLevelPages().ToListAsync();
        Assert.IsNotNull(pages);
        Assert.AreNotEqual(0, pages.Count);
        Assert.AreNotEqual(null, pages[0].Title());

        //var firstPage = pages[0];
        //var titlePropertyId = firstPage.Properties!.First().Value.Id;
        //var titleProperty = await session.GetPageProperty(firstPage.Id, titlePropertyId).FirstOrDefaultAsync();
        //Assert.IsNotNull((titleProperty as TitlePropertyItem)?.Title[0].PlainText);
    }
        
        
    [TestMethod]
    public async Task TestGetSyndicationFeed()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        var pages = await session.GetTopLevelPages()
            .WhereAwait(async p => p.Title().Title[0].PlainText == "Public blog")
            .ToListAsync();
        Assert.IsNotNull(pages);
        Assert.AreEqual(1, pages.Count);

        var feed = await session.GetSyndicationFeed(pages[0], new ("https://"));
        Assert.IsNotNull(feed?.Items);
        Assert.AreNotEqual(0, feed.Items.Count());
    }
        
    [TestMethod]
    public async Task TestGetHtml()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        var page = await session.Search(filterOptions: FilterOptions.ObjectPage)
            .WhereAwait(async p => p.Title().Title[0].PlainText == "Procrastination")
            .FirstAsync();
        Assert.IsNotNull(page);

        var html = await session.GetHtml(page);
        Assert.IsFalse(string.IsNullOrWhiteSpace(html));
    }

        
    [TestMethod]
    public async Task TestGetPublicBlogContent()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        var pages = await session.GetTopLevelPages().ToListAsync();
        Assert.IsNotNull(pages);
        Assert.AreNotEqual(0, pages.Count);

        Assert.IsNotNull((pages[0].Properties["title"] as TitlePropertyItem).Title[0].PlainText);
    }
    
    [TestMethod]
    public async Task TestPageAndChildrenDeserialization()
    {
        var pageJson = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.json"));
        var page = JsonSerializer.Deserialize<PaginationResult<Page>>(pageJson, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(page);

        var blocksJson = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "JsonData", "AboutThis.Children.json"));
        var blocks = JsonSerializer.Deserialize<PaginationResult<Block>>(blocksJson, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(blocks);
    }
    
    [TestMethod]
    [Ignore("Run this manually to create the json files")]
    public async Task TestPageAndChildrenSerialization()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        var page = await session.Search(query: "About this", filterOptions: FilterOptions.ObjectPage).FirstAsync();
        Assert.AreEqual("About this", page?.Title()?.Title.FirstOrDefault()?.PlainText);

        var blocks = await session.GetBlockChildren(page.Id).ToListAsync();
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

        var pageJson = JsonSerializer.Serialize(page, HttpNotionSession.NotionJsonSerializationOptions);
        var blocksJson = JsonSerializer.Serialize(blocks, HttpNotionSession.NotionJsonSerializationOptions);
        var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        await File.WriteAllTextAsync(Path.Combine(path, "AboutThis.full.json"), pageJson);
        await File.WriteAllTextAsync(Path.Combine(path, "AboutThis.full.children.json"), blocksJson);
        
        var page2 = JsonSerializer.Deserialize<Page>(pageJson, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(page2);
        var blocks2 = JsonSerializer.Deserialize<List<Block>>(blocksJson, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(blocks2);
        Assert.AreEqual(blocks.Count,blocks2.Count);
    }

    [TestMethod]
    public void TestNotionUtils()
    {
        var baseUri = new Uri("https://vapolia.notion.site");
        var title = "Creating a good Xamarin Forms Control Part 2 UI Day 3";
        var pageId = "a1744f66-81bf-41a5-b93d-70cbdf6bc3db";
        var url = NotionUtils.GetPageUri(pageId, title, baseUri).ToString();
        Assert.AreEqual("https://vapolia.notion.site/Creating-a-good-Xamarin-Forms-Control-Part-2-UI-Day-3-a1744f6681bf41a5b93d70cbdf6bc3db", url);
    }

}