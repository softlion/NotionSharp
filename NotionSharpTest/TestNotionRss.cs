﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotionSharp;

namespace NotionSharpTest;

[TestClass]
public class TestNotionRss
{
    /// <summary>
    /// get root pages of 1st space and put them in a RSS feed
    /// </summary>
    [TestMethod]
    public async Task GetRssFeedFromRootPagesOfFirstSpace()
    {
        var sessionInfo = TestUtils.CreateUnofficialNotionSessionInfo();
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
        var sessionInfo = TestUtils.CreateUnofficialNotionSessionInfo();
        var notionSession = new NotionSession(sessionInfo);

        //Get first notion "space"
        var userContent = await notionSession.LoadUserContent();
        var space = userContent.RecordMap.Space.First().Value;
        var spacePages = space.Pages;

        //Get sub-pages of the "public blog" page, which makes the content of the blog
        var blogId = spacePages.First(pageId => userContent.RecordMap.Block[pageId].Title == "Public blog");
        var blog = userContent.RecordMap.Block[blogId];
        var blogPages = blog.Content;

        //Convert the blogPages into a RSS feed
        var feed = await notionSession.GetSyndicationFeed(space.Id, blogPages, maxBlocks: 100);
        feed.Title = new (blog.Title);

        Assert.IsNotNull(feed);
        Assert.IsTrue(feed.Items.Any());
        Assert.IsFalse(feed.Items.Any(item => item.Title?.Text == "Work In Progress"));
    }
}