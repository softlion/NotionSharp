# NotionSharp

[Notion.so](https://notion.so) is an all-in-one workspace to write things, with the best simple and powerful "wysiwyg" editor, with color highlightning for code blocks, easy video interations, and much more. It includes an offline windows application to peacefully write your blog's content.

This is an unofficial [notion.so](https://notion.so) APIv3 library that enables you, among other things, to get a list of pages as a RSS feed, as notion has no RSS feed as of 2020/04/30.

[![NuGet][nuget-img]][nuget-link]

![notion-img]

[nuget-link]: https://www.nuget.org/packages/Softlion.NotionSharp/
[nuget-img]: https://img.shields.io/nuget/v/Softlion.NotionSharp
[notion-img]: https://github.com/softlion/NotionSharp/raw/master/cover.png

## Notion high-level model

Spaces  
|- Pages  
....|-- Sub-Pages

There seems to be only one space per (personnal) account.  
All pages are blocks. All paragraphs in a page are also blocks. Block.Type contains a block's type.  
For a Page block, Block.Content contains the ids of the child pages.  
All item's ids are Guids.

Known block types are:
- page, collection_view_page
- text, image, sub_header, quote, code, bookmark

## Usage

Create a session:

```csharp
    var sessionInfo = new NotionSessionInfo 
    {
        TokenV2 = "paste here the content of the token_v2 cookie set after you logged in notion.so",
        NotionBrowserId = Guid.Parse("00000000-0000-0000-0000-000000000000"), //paste the content of the notion_browser_id cookie
        NotionUserId = Guid.Parse("00000000-0000-0000-0000-000000000000") //paste the content of the notion_user_id cookie
    };

    var session = new NotionSession(sessionInfo);
```

Load content and info of main space:

```csharp
    var userContent = await session.LoadUserContent();
    var html = userContent.RecordMap.GetHtml(throwIfBlockMissing: false);
```
    
Get a RSS representation of all root pages of the main space.  
This also transforms notion data to HTML:

```csharp
    var rssFeed = await session.GetSyndicationFeed();
```
    
    
Get a RSS representation from the sub-pages of a page:

```csharp
    var space = userContent.RecordMap.Space.First().Value;
    var firstPage = userContent.RecordMap.Block[space.Pages[0]];
    var subPages = firstPage.Content;

    var feedPublicBlog = await session.GetSyndicationFeed(subPages);
    feedPublicBlog.Title = new TextSyndicationContent(firstPage.Title);
```
