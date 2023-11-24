﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp.ApiClient.Lib;
using NotionSharp.ApiClient.Tests.Lib;

namespace NotionSharp.ApiClient.Tests;

[TestClass]
public class TestNotionOfficial
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
    public async Task TestGetTopLevelPages()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        var pages = await session.GetTopLevelPages().ToListAsync();
        Assert.IsNotNull(pages);
        Assert.AreNotEqual(0, pages.Count);

        var firstPage = pages[0];
        var propertyId = "";
        var pages0property = await session.GetPageProperty(firstPage.Id, propertyId).FirstOrDefaultAsync();
        Assert.IsNotNull((pages0property as TitlePropertyItem)?.Title.Title[0].PlainText);
    }
        
        
    [TestMethod]
    public async Task TestGetSyndicationFeed()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        var pages = await session.GetTopLevelPages()
            .WhereAwait(async p =>
            {
                throw new NotImplementedException("propertyId not set");
                var propertyId = "";
                var titleProp = await session.GetPageProperty(p.Id, propertyId).FirstOrDefaultAsync();
                return (titleProp as TitlePropertyItem)?.Title?.Title?.FirstOrDefault().PlainText == "Public blog";
            })
            .ToListAsync();
        Assert.IsNotNull(pages);
        Assert.AreEqual(1, pages.Count);

        var feed = await session.GetSyndicationFeed(pages[0]);
        Assert.IsNotNull(feed?.Items);
        Assert.AreNotEqual(0, feed.Items.Count());
    }
        
    [TestMethod]
    public async Task TestGetHtml()
    {
        var session = new NotionSession(TestUtils.CreateOfficialNotionSessionInfo());
        var page = await session.Search(filterOptions: FilterOptions.ObjectPage)
            .WhereAwait(async p =>
            {
                throw new NotImplementedException("propertyId not set");
                var propertyId = "";
                var titleProp = await session.GetPageProperty(p.Id, propertyId).FirstOrDefaultAsync();
                return (titleProp as TitlePropertyItem)?.Title?.Title?.FirstOrDefault().PlainText == "Procrastination";
            })
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

        throw new NotImplementedException("propertyId not set");
        var propertyId = "";
        var pages0property = await session.GetPageProperty(pages[0].Id, propertyId).FirstOrDefaultAsync();
        Assert.IsNotNull((pages0property as TitlePropertyItem).Title.Title[0].PlainText);
    }


    [TestMethod]
    public void TestDeserializeDictionary()
    {
        var json = "{\"title\": \"id\"}";
        var p1 = JsonSerializer.Deserialize<Dictionary<string,object>>(json, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(p1);
            
        json = "{\"properties\":{\"title\": \"id\"}}";
        var p2 = JsonSerializer.Deserialize<ObjectWithProperties>(json, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(p2);
        Assert.IsNotNull(p2.Properties);
        Assert.AreEqual(1, p2.Properties.Count);
        Assert.AreEqual("id", p2.Properties["title"].GetString());

        json = "{\"object\":\"page\",\"id\":\"18dfbe55-5d7c-416e-9485-7855d4a3949e\",\"created_time\":\"2020-04-30T13:42:00.000Z\",\"last_edited_time\":\"2021-06-22T09:58:00.000Z\",\"parent\":{\"type\":\"workspace\",\"workspace\":true},\"archived\":false,\"properties\":{\"title\":{\"id\":\"title\",\"type\":\"title\",\"title\":[{\"type\":\"text\",\"text\":{\"content\":\"Public blog\",\"link\":null},\"annotations\":{\"bold\":false,\"italic\":false,\"strikethrough\":false,\"underline\":false,\"code\":false,\"color\":\"default\"},\"plain_text\":\"Public blog\",\"href\":null}]}}}";
        var p3 = JsonSerializer.Deserialize<ObjectWithProperties>(json, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(p3);
        Assert.IsNotNull(p3.Properties);
        var p4 = JsonSerializer.Deserialize<Page>(json, HttpNotionSession.NotionJsonSerializationOptions);
        Assert.IsNotNull(p4);
        Assert.IsNotNull(p4.Parent);
        //Assert.IsNotNull(p4.Properties);
        //Assert.IsNotNull(p4.Title);
        //var title = p4.Properties["title"];
        //Assert.IsNotNull(title);
        //var tt = title.ToObject<Page.PropertyTitle>(HttpNotionSession.NotionJsonSerializationOptions);
        // Assert.IsNotNull(tt);
        // Assert.IsNotNull(tt.Title);
        // Assert.AreNotEqual(0, tt.Title.Count);
    }
        
    class ObjectWithProperties
    {
        public Dictionary<string,JsonElement> Properties { get; set; }
    }
}